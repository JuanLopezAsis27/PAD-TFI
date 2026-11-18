# ArcaSim — Rol 2: Servicio SOAP y Autenticación

## 1. Objetivo

El Rol 2 implementa el servicio web SOAP del simulador ArcaSim, inspirado en las operaciones de facturación electrónica WSFEv1 de ARCA.

Sus responsabilidades principales son:

- Exponer operaciones SOAP mediante SoapCore.
- Publicar el contrato WSDL.
- Autenticar solicitudes mediante API keys asociadas a una CUIT.
- Validar las credenciales antes de ejecutar operaciones protegidas.
- Traducir solicitudes SOAP a los objetos y servicios de la aplicación.
- Devolver respuestas XML con resultados, códigos de autorización simulados y errores.

La validación de comprobantes y la emisión de CAE simulados se realizan mediante los servicios del proyecto; la persistencia utiliza PostgreSQL.

## 2. Tecnologías utilizadas

- .NET 10 y ASP.NET Core.
- SoapCore.
- C#.
- Entity Framework Core.
- PostgreSQL.
- Docker.
- SHA-256 para el almacenamiento de hashes de API keys.
- xUnit para pruebas automatizadas.

## 3. Endpoint del servicio

**Servicio SOAP local:**

`http://localhost:5080/wsfev1/service.asmx`

**WSDL:**

`http://localhost:5080/wsfev1/service.asmx?WSDL`

**Namespace SOAP:**

`http://ar.gov.afip.dif.FEV1/`

Las solicitudes de prueba utilizan SOAP 1.1 mediante HTTP POST.

## 4. Operaciones implementadas

| Operación | Descripción |
|---|---|
| FEDummy | Verificación básica del servicio |
| FECompUltimoAutorizado | Consulta del último número autorizado |
| FECompConsultar | Consulta de un comprobante autorizado |
| FECAESolicitar | Solicitud de autorización y generación de CAE simulado |
| FEParamGetTiposCbte | Consulta de tipos de comprobante |
| FEParamGetTiposIva | Consulta de alícuotas de IVA |
| FEParamGetCondicionIvaReceptor | Consulta de condiciones de IVA del receptor |
| FECompTotXRequest | Consulta del límite de comprobantes por solicitud |
| FEParamGetTiposDoc | Consulta de tipos de documento |
| FEParamGetTiposConcepto | Consulta de conceptos de facturación |
| FEParamGetPtosVenta | Consulta de puntos de venta de una CUIT |
| FEParamGetTiposMonedas | Consulta de monedas publicadas por el simulador |

Actualmente se publican 12 operaciones SOAP.

## 5. Autenticación

Las operaciones protegidas reciben un elemento `Auth` con los siguientes campos:

| Campo | Tipo | Descripción |
|---|---|---|
| Token | string | API key proporcionada al cliente |
| Sign | string | Se mantiene vacío en el mecanismo actual |
| Cuit | long | CUIT del contribuyente asociado |

Ejemplo:

```xml
<Auth>
  <Token>API_KEY_PROPORCIONADA_AL_CLIENTE</Token>
  <Sign></Sign>
  <Cuit>30712345671</Cuit>
</Auth>
```

El mecanismo implementado es una autenticación propia del simulador mediante API keys. **No constituye una implementación de la autenticación oficial WSAA de ARCA.**

### Validación de una API key

1. El cliente envía el token y la CUIT.
2. El servicio calcula el hash SHA-256 del token.
3. Se consulta el hash almacenado en PostgreSQL.
4. Se comprueba que la clave exista y no esté revocada.
5. Se comprueba que la CUIT coincida con la asociada a la clave.
6. Si la validación es exitosa, se ejecuta la operación solicitada.

La tabla `api_keys_soap` almacena el hash, la CUIT, el estado de revocación y las fechas de creación y revocación.

La clave original no se almacena en la base de datos.

La generación y revocación se implementan mediante `IAdministradorApiKeysSoap`. No se exponen públicamente como operaciones SOAP administrativas.

### Errores de autenticación

| Código | Situación |
|---|---|
| 600 | Credenciales ausentes, clave inválida o revocada, o mecanismo de firma no disponible |
| 601 | La CUIT no coincide con la asociada a la clave |

Las respuestas de error se devuelven mediante elementos `Errors` y `Err`.

## 6. Emisión de comprobantes

La operación `FECAESolicitar` recibe:

- Datos de autenticación.
- Cabecera del comprobante.
- Detalle de la factura.
- Importes y datos fiscales admitidos por el simulador.

El servicio realiza las validaciones iniciales, convierte los datos SOAP a objetos del dominio e invoca `ComprobanteServicio` para registrar y autorizar simuladamente el comprobante.

### Resultado de autorización

- `A`: comprobante aprobado.
- `R`: comprobante rechazado.

Cuando una factura es aprobada, la respuesta incluye un CAE simulado y su fecha de vencimiento.

Los CAE generados por ArcaSim no tienen validez fiscal ante ARCA.

### Ejemplo de prueba realizada

Se emitió satisfactoriamente una factura B de prueba con:

- CUIT emisora: 30712345671.
- Punto de venta: 1.
- Número: 2.
- Importe neto: 100.
- IVA: 21.
- Importe total: 121.
- Moneda: PES.

El servidor devolvió un resultado aprobado y un CAE simulado. Posteriormente se recuperó el comprobante mediante `FECompConsultar` y se comprobó que `FECompUltimoAutorizado` reflejaba la numeración actualizada.

## 7. Validaciones probadas

Se comprobaron los siguientes escenarios:

| Escenario | Resultado |
|---|---|
| API key válida y CUIT correcta | Acceso permitido |
| API key inexistente | Error 600 |
| API key revocada | Error 600 |
| API key válida con CUIT diferente | Error 601 |
| Fecha de comprobante inválida | Rechazo |
| Suma de importes incorrecta | Rechazo |
| Número de comprobante duplicado | Rechazo |
| Emisión de factura válida | Autorización con CAE |
| Consulta de comprobante existente | Recuperación de datos y CAE |

También se ejecutó una prueba HTTP de autenticación sobre las 11 operaciones protegidas, con 11 resultados aprobados y ninguna falla.

## 8. Catálogos

Los catálogos disponibles se basan en los enums y capacidades actuales del simulador.

Entre ellos:

- Tipos de comprobante.
- Alícuotas de IVA.
- Condiciones de IVA del receptor.
- Tipos de documento.
- Conceptos de facturación.
- Puntos de venta registrados.
- Moneda PES.

Algunos catálogos están reducidos respecto del servicio oficial de ARCA. Los datos no disponibles, como ciertas fechas de vigencia, no se inventan.

## 9. Limitaciones actuales

Esta implementación representa un subconjunto funcional de WSFEv1.

Actualmente:

- La autenticación utiliza API keys, no WSAA.
- `FECAESolicitar` admite un comprobante por solicitud.
- No se implementaron operaciones CAEA.
- No se admiten comprobantes asociados, tributos ni campos opcionales en la emisión actual.
- La moneda PES es la única publicada en el catálogo de monedas.
- No se implementaron todos los catálogos del servicio oficial.
- La consulta de comprobantes todavía no expone la fecha real de procesamiento si no está disponible en el dominio.
- No se garantiza compatibilidad completa con todos los clientes que consumen el WSDL oficial de ARCA.

## 10. Ejecución local

Es necesario disponer de PostgreSQL configurado y de las migraciones correspondientes aplicadas.

Desde la raíz del repositorio:

```powershell
docker compose up -d
```

Iniciar la API:

```powershell
dotnet run --project src/ArcaSim.Api/ArcaSim.Api.csproj --urls http://localhost:5080
```

Consultar el WSDL desde el navegador:

`http://localhost:5080/wsfev1/service.asmx?WSDL`

## 11. Pruebas

Compilar la solución:

```powershell
dotnet build
```

Ejecutar todas las pruebas:

```powershell
dotnet test tests/ArcaSim.Tests/ArcaSim.Tests.csproj
```

Ejecutar las pruebas de serialización de catálogos SOAP:

```powershell
dotnet test tests/ArcaSim.Tests/ArcaSim.Tests.csproj --filter "FullyQualifiedName~CatalogosSoapTests"
```

## 12. Consideraciones de seguridad

- No almacenar API keys originales en PostgreSQL.
- No incluir claves reales en el repositorio.
- No publicar las credenciales en archivos de documentación.
- Restringir la generación y revocación de claves a administradores autorizados.
- Utilizar HTTPS fuera del entorno local.
- No exponer el servicio como sistema fiscal oficial.

## 13. Integración con otros roles

**Rol 1 — Dominio y lógica de negocio:** proporciona entidades, enumeraciones y validaciones de comprobantes.

**Rol 2 — Servicio SOAP y autenticación:** expone los contratos XML, autentica y adapta las solicitudes a los servicios de aplicación.

**Rol 3 — Persistencia:** mantiene los repositorios, el contexto de Entity Framework Core y PostgreSQL.

**Rol 4 — Portal:** podrá consumir las operaciones SOAP y utilizar los mecanismos administrativos que se habiliten con autorización adecuada.

La responsabilidad del Rol 2 es mantener el contrato SOAP, la autenticación y el mapeo entre los mensajes XML y la lógica de aplicación.