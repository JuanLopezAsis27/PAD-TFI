# Integración con RENAPER

ArcaSim usa el servicio **Renaper Validation** de otro grupo para validar la identidad de las
personas humanas que se dan de alta como contribuyentes.

- Documentación del servicio: <https://renaper-frontend-pad.vercel.app/documentacion>
- URL de la API: `https://146.181.31.154.nip.io/`

## Qué hace el servicio (y qué no)

El servicio **no devuelve los datos de la persona**. Le mandamos un DNI con el nombre, el apellido y
una edad mínima, y responde solo si coinciden:

```http
POST /api/v1/personas/validar
Content-Type: application/json
X-API-Key: <API key de ArcaSim>

{ "dni": 40111222, "nombre": "Lucas Matias", "apellido": "Gomez", "edadMinima": 18 }
```

```json
{ "datosCoinciden": true, "cumpleEdadMinima": true }
```

La persona se acepta solo si los dos valores son `true`. Una respuesta con un campo faltante o de
otro tipo no se toma como válida.

## Cómo lo usa ArcaSim

`ContribuyenteServicio.RegistrarPersonaHumanaAsync(cuit, nombre, apellido, condicionIva)`:

1. Valida la CUIT (dígito verificador) y que su prefijo sea de persona humana: 20, 23, 24 o 27.
2. Controla que la CUIT no esté registrada. Se hace **antes** de llamar a RENAPER porque cada
   consulta consume un crédito.
3. Toma el DNI de la CUIT (los 8 dígitos del medio: `20-40111222-8` → `40111222`).
4. Consulta a RENAPER con el nombre, el apellido y una edad mínima de **18 años**.
5. Si los dos valores son `true`, registra al contribuyente con el nombre `"Apellido, Nombre"`.

`ContribuyenteServicio.Registrar(contribuyente)` queda solo para personas jurídicas: si recibe una
persona humana, la rechaza.

Decisiones que salen de cómo funciona el servicio:

- **La CUIT se valida, no se calcula.** El prefijo depende del sexo, y el servicio no lo maneja.
- **El nombre no se autocompleta.** La persona lo escribe en el alta y RENAPER confirma si coincide.
- **No se validan los receptores de las facturas.** `FECAESolicitar` (igual que en ARCA) informa el
  DNI del receptor pero no su nombre, y el servicio lo exige. Además, haría que facturar dependa de
  un servicio externo que cobra por consulta.
- **No se puede saber si la persona falleció:** el servicio aclara que no acredita el estado vital.

## Errores

Cuando RENAPER no da una respuesta válida, el alta se rechaza con `RenaperNoDisponibleException` y
un mensaje que dice qué revisar. El alta nunca se acepta sin una respuesta válida.

| Situación | Mensaje |
|---|---|
| `401` / `403` | RENAPER rechazó la API key: revisar `Renaper:ApiKey` (quizás se generó una nueva). |
| `402` | RENAPER no tiene saldo de consultas: hay que comprar créditos en su portal. |
| `400` / `422` | RENAPER rechazó los datos enviados. |
| Otro código | RENAPER respondió con un error (código). |
| Sin conexión o sin respuesta en `TimeoutSegundos` | No se pudo conectar con RENAPER / RENAPER no respondió a tiempo. |
| Respuesta incompleta o que no es JSON | RENAPER devolvió una respuesta incompleta / que no se pudo interpretar. |

Si los datos no coinciden o no cumple la edad, el servicio lanza `InvalidOperationException` con
el motivo. Para el portal (Rol 4), una forma de traducirlo a HTTP: `ArgumentException` → 400,
`InvalidOperationException` → 409 o 422, `RenaperNoDisponibleException` → 503.

## Configuración

Sección `Renaper` de `appsettings`:

| Clave | Qué es | Producción | Desarrollo |
|---|---|---|---|
| `Modo` | `Http` (servicio real) o `Stub` (simulado, ver abajo) | `Http` | `Stub` |
| `UrlBase` | URL de la API | `https://146.181.31.154.nip.io/` | la misma |
| `ApiKey` | API key de ArcaSim. **Es un secreto: nunca va en git** | variable de entorno `Renaper__ApiKey` | user-secrets |
| `TimeoutSegundos` | Tiempo máximo de espera | `10` | `10` |

En modo `Http` la API **no arranca** si falta `UrlBase` o `ApiKey`, para no descubrir el problema
recién en la primera alta.

Para usar el servicio real en desarrollo (cada alta consume un crédito):

```bash
dotnet user-secrets set "Renaper:Modo" "Http" --project src/ArcaSim.Api
dotnet user-secrets set "Renaper:ApiKey" "rnp_test_..." --project src/ArcaSim.Api
```

En Docker, crear un archivo `.env` en la raíz (git lo ignora) con `RENAPER_MODO=Http` y
`RENAPER_API_KEY=...`.

### Obtener la API key (una sola vez, lo hace una persona del equipo)

1. Crear la cuenta de ArcaSim en el portal del servicio.
2. Comprar créditos: el paquete es de 1.000 consultas y se paga con la cuenta de prueba de Mercado
   Pago que indica su documentación.
3. En *Gestionar cuenta* → *API key* → *Crear API key*. Se ve completa una sola vez: copiarla y
   guardarla en la configuración del servidor. Generar una nueva invalida la anterior.

## RENAPER simulado (stub)

En desarrollo, en Docker y en los tests se usa `RenaperClienteSimulado`. Responde igual que el
servicio real pero con estas personas ficticias, sin red y sin consumir créditos. Compara nombre y
apellido sin distinguir mayúsculas, tildes ni espacios de más.

| CUIT para el alta | DNI | Nombre | Apellido | Nacimiento | Sirve para probar |
|---|---|---|---|---|---|
| `20401112228` | 40111222 | Lucas Matias | Gomez | 14/03/1997 | Un alta correcta (es el ejemplo de su documentación) |
| `27287654311` | 28765431 | María | Ejemplo | 02/06/1981 | Ya está en los datos de prueba: da "ya existe" |
| `20301234563` | 30123456 | Juan Carlos | Pérez | 21/09/1983 | Un alta correcta |
| `27359876543` | 35987654 | Ana Laura | Fernández | 05/11/1991 | Nombre incompleto ("Ana"): datos no coinciden |
| `23501112224` | 50111222 | Sofía | Menor | 10/05/2012 | Menor de edad: no cumple la edad mínima |

Cualquier otro DNI responde "los datos no coinciden", igual que el servicio real cuando el DNI no
existe. Los datos de prueba de la base (`DatosDePrueba`) no pasan por RENAPER.

## Preguntas pendientes para el otro grupo

- ¿Qué tan estricta es la comparación del nombre (mayúsculas, tildes, segundo nombre)?
- ¿Qué cuerpo devuelven los errores? El `401` llega vacío.
- ¿Tienen una lista de DNIs ficticios para probar?
- ¿La URL con la IP va a ser estable?
