using System.Globalization;
using ArcaSim.Web.Servicios;
using Microsoft.AspNetCore.Components;

namespace ArcaSim.Web.Components.Pages;

/// <summary>
/// Datos y ejemplos de la página de documentación del web service. Los pedidos y las respuestas son
/// los que devuelve ArcaSim.Api (se copiaron de llamadas reales); los programas de ejemplo se
/// probaron contra la API: piden el último número y obtienen un CAE.
/// </summary>
public partial class Documentacion
{
    public const string Namespace = "http://ar.gov.afip.dif.FEV1/";
    private const string CuitEjemplo = "30712345671";

    [Inject]
    private DireccionesWebService Direcciones { get; set; } = default!;

    private string UrlServicio => Direcciones.Servicio;

    private string UrlWsdl => Direcciones.Wsdl;

    /// <summary>Fecha de hoy en el formato de ARCA, para que los ejemplos se puedan enviar tal cual.</summary>
    private string Hoy { get; } = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    private (string Etiqueta, string Valor, string? Nota)[] DatosConexion =>
    [
        ("Servicio", UrlServicio, "Los pedidos van por POST a esta dirección."),
        ("WSDL", UrlWsdl, null),
        ("Namespace", Namespace, null),
        ("SOAPAction", Namespace + "FECAESolicitar", "Header HTTP: el namespace más el nombre de la operación, entre comillas."),
        ("Content-Type", "text/xml; charset=utf-8", "SOAP 1.1.")
    ];

    // ===== Pestañas de código =====

    private PestanasCodigo.Pestana[] PasoUltimo =>
    [
        new("Pedido", PedidoUltimoAutorizado),
        new("Respuesta", RespuestaUltimoAutorizado)
    ];

    private PestanasCodigo.Pestana[] PasoCae =>
    [
        new("Pedido", PedidoCae),
        new("Respuesta aprobada (A)", RespuestaCaeAprobada),
        new("Respuesta rechazada (R)", RespuestaCaeRechazada)
    ];

    private PestanasCodigo.Pestana[] PasoConsultar =>
    [
        new("Pedido", PedidoConsultar),
        new("Respuesta", RespuestaConsultar)
    ];

    private PestanasCodigo.Pestana[] Clientes =>
    [
        new("C#", EjemploCSharp, "C# · .NET 8 o superior"),
        new("Python", EjemploPython, "Python · zeep"),
        new("Node.js", EjemploNode, "Node.js · soap"),
        new("curl", EjemploCurl, "Bash · curl")
    ];

    // ===== Tablas =====

    private sealed record Seccion(string Id, string Titulo);

    private static readonly Seccion[] Indice =
    [
        new("conexion", "Datos de conexión"),
        new("alta", "1. Alta en el portal"),
        new("autenticacion", "2. Autenticación"),
        new("flujo", "3. Emitir una factura"),
        new("campos", "Campos de FECAESolicitar"),
        new("operaciones", "Operaciones"),
        new("codigos", "Tablas de códigos"),
        new("errores", "Errores"),
        new("clientes", "Ejemplos de código"),
        new("diferencias", "Diferencias con ARCA")
    ];

    private sealed record Operacion(string Nombre, string Para, string Parametros, bool PideAuth);

    private static readonly Operacion[] Operaciones =
    [
        new("FEDummy", "Ver si el servicio está en línea (AppServer, DbServer y AuthServer en OK).", "—", false),
        new("FECompUltimoAutorizado", "Último número autorizado para un punto de venta y tipo. El próximo es ese + 1.", "PtoVta, CbteTipo", true),
        new("FECAESolicitar", "Autorizar un comprobante y obtener el CAE con su vencimiento.", "FeCAEReq", true),
        new("FECompConsultar", "Datos y CAE de un comprobante ya autorizado.", "FeCompConsReq (CbteTipo, CbteNro, PtoVta)", true),
        new("FECompTotXRequest", "Cuántos comprobantes acepta un FECAESolicitar (hoy, 1).", "—", true),
        new("FEParamGetPtosVenta", "Puntos de venta de la CUIT del Auth.", "—", true),
        new("FEParamGetTiposCbte", "Tipos de comprobante.", "—", true),
        new("FEParamGetTiposDoc", "Tipos de documento del receptor.", "—", true),
        new("FEParamGetTiposIva", "Alícuotas de IVA.", "—", true),
        new("FEParamGetCondicionIvaReceptor", "Condiciones frente al IVA del receptor, por clase de comprobante.", "ClaseCmp (A, B, C o vacío)", true),
        new("FEParamGetTiposConcepto", "Conceptos (productos, servicios o ambos).", "—", true),
        new("FEParamGetTiposMonedas", "Monedas publicadas.", "—", true)
    ];

    private sealed record Campo(string Nombre, string Tipo, string Descripcion);

    private static readonly Campo[] CamposCabecera =
    [
        new("CantReg", "int", "Cantidad de comprobantes del pedido. Siempre 1."),
        new("PtoVta", "int", "Punto de venta habilitado del emisor."),
        new("CbteTipo", "int", "Tipo de comprobante (ver tabla de códigos).")
    ];

    private static readonly Campo[] CamposDetalle =
    [
        new("Concepto", "int", "1 productos, 2 servicios, 3 productos y servicios."),
        new("DocTipo / DocNro", "int / long", "Documento del receptor. Consumidor final sin identificar: 99 y 0."),
        new("CbteDesde / CbteHasta", "long", "Número del comprobante: los dos iguales (último autorizado + 1)."),
        new("CbteFch", "yyyyMMdd", "Fecha del comprobante: hasta 5 días antes o después de hoy (10 si son servicios)."),
        new("ImpTotal", "decimal", "Total = ImpNeto + ImpTotConc + ImpOpEx + ImpIVA + ImpTrib."),
        new("ImpTotConc", "decimal", "Importe no gravado. En clase C, 0."),
        new("ImpNeto", "decimal", "Importe neto gravado. En clase C es el subtotal."),
        new("ImpOpEx", "decimal", "Importe exento. En clase C, 0."),
        new("ImpTrib", "decimal", "Tributos. Por ahora, siempre 0."),
        new("ImpIVA", "decimal", "Suma del IVA de las alícuotas. En clase C, 0."),
        new("FchServDesde / FchServHasta / FchVtoPago", "yyyyMMdd", "Obligatorias si Concepto es 2 o 3; no se envían si es 1."),
        new("MonId / MonCotiz", "string / decimal", "Moneda y cotización: PES y 1."),
        new("Iva → AlicIva (Id, BaseImp, Importe)", "lista", "Una entrada por alícuota, sin repetir. Obligatoria en A y B; no se envía en C."),
        new("CondicionIVAReceptorId", "int", "Condición frente al IVA del receptor (RG 5616). Obligatoria.")
    ];

    private static readonly (string Codigo, string Descripcion)[] TiposComprobante =
    [
        ("1", "Factura A"), ("2", "Nota de Débito A"), ("3", "Nota de Crédito A"),
        ("6", "Factura B"), ("7", "Nota de Débito B"), ("8", "Nota de Crédito B"),
        ("11", "Factura C"), ("12", "Nota de Débito C"), ("13", "Nota de Crédito C")
    ];

    private static readonly (string Codigo, string Descripcion)[] TiposDocumento =
    [
        ("80", "CUIT"), ("86", "CUIL"), ("96", "DNI"), ("99", "Sin identificar (consumidor final, número 0)")
    ];

    private static readonly (string Codigo, string Descripcion)[] Alicuotas =
    [
        ("3", "0 %"), ("4", "10,5 %"), ("5", "21 %"), ("6", "27 %"), ("8", "5 %"), ("9", "2,5 %")
    ];

    private static readonly (string Codigo, string Descripcion, string Clases)[] CondicionesReceptor =
    [
        ("1", "IVA Responsable Inscripto", "A, C"),
        ("4", "IVA Sujeto Exento", "B, C"),
        ("5", "Consumidor Final", "B, C"),
        ("6", "Responsable Monotributo", "A, C"),
        ("7", "Sujeto No Categorizado", "B, C")
    ];

    private static readonly (string Codigo, string Descripcion)[] Conceptos =
    [
        ("1", "Productos"), ("2", "Servicios"), ("3", "Productos y servicios")
    ];

    private sealed record ErrorFrecuente(string Codigo, string Significa, string QueHacer);

    private static readonly ErrorFrecuente[] ErroresGenerales =
    [
        new("600", "Falta el Auth, la API key no existe o fue revocada, o se envió Sign.", "Revisá el Token y dejá Sign vacío. Si regeneraste la clave, la anterior ya no sirve."),
        new("601", "La CUIT del Auth no es la de la API key.", "Mandá en Cuit la CUIT para la que generaste la clave."),
        new("400", "Pedido incompleto o mal formado, o el comprobante no pasó una regla de ARCA.", "El Msg empieza con el código de ARCA (por ejemplo, \"10048: ...\"): ver la tabla de abajo."),
        new("401", "FECompConsultar: el comprobante no existe o no está autorizado.", "Revisá tipo, punto de venta y número."),
        new("500", "No se pudo confirmar la autorización.", "Consultá el comprobante con FECompConsultar antes de reintentar.")
    ];

    private static readonly ErrorFrecuente[] RechazosFrecuentes =
    [
        new("10016", "El número no es el siguiente al último autorizado, o la fecha es anterior a la del último.", "Pedí FECompUltimoAutorizado y usá ese número + 1."),
        new("DOM-014", "Ese número ya está autorizado (por ejemplo, se reenvió un pedido que ya había salido bien).", "Consultalo con FECompConsultar; para uno nuevo, usá el último + 1."),
        new("10048", "El total no es la suma de los importes.", "ImpTotal = ImpNeto + ImpTotConc + ImpOpEx + ImpIVA + ImpTrib."),
        new("10051", "El IVA de una alícuota no corresponde a su base imponible.", "Importe = BaseImp × alícuota (redondeado a 2 decimales)."),
        new("10023", "ImpIVA no es la suma de las alícuotas.", "Sumá el Importe de cada AlicIva."),
        new("10013", "Las facturas A exigen la CUIT del receptor.", "DocTipo 80 y DocNro con la CUIT."),
        new("10071", "Los comprobantes C no llevan IVA.", "No mandes el bloque Iva y poné ImpIVA en 0."),
        new("10243", "La condición IVA del receptor no corresponde a la clase del comprobante.", "Ver la tabla de condiciones (A: 1 o 6; B: 4, 5 o 7)."),
        new("10246", "Falta CondicionIVAReceptorId.", "Es obligatorio desde la RG 5616."),
        new("DOM-003", "El tipo de comprobante no corresponde a la condición del emisor.", "Responsable inscripto: A o B. Monotributista o exento: C."),
        new("DOM-004", "Fecha fuera de rango o fechas de servicio incompletas.", "CbteFch cerca de hoy; con servicios, las tres fechas de servicio."),
        new("DOM-010", "El punto de venta no existe, está deshabilitado o no es del emisor.", "Habilitalo en el portal o consultá FEParamGetPtosVenta.")
    ];

    // ===== Pedidos y respuestas (SOAP 1.1) =====

    private string XmlAuth => $"""
        <ar:Auth>
          <ar:Token>TU_API_KEY</ar:Token>   <!-- la API key que generaste en el portal -->
          <ar:Sign></ar:Sign>               <!-- vacío -->
          <ar:Cuit>{CuitEjemplo}</ar:Cuit>      <!-- la CUIT de esa API key -->
        </ar:Auth>
        """;

    private string PedidoUltimoAutorizado => $"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ar="{Namespace}">
          <soapenv:Body>
            <ar:FECompUltimoAutorizado>
              <ar:Auth>
                <ar:Token>TU_API_KEY</ar:Token>
                <ar:Sign></ar:Sign>
                <ar:Cuit>{CuitEjemplo}</ar:Cuit>
              </ar:Auth>
              <ar:PtoVta>1</ar:PtoVta>
              <ar:CbteTipo>6</ar:CbteTipo>
            </ar:FECompUltimoAutorizado>
          </soapenv:Body>
        </soapenv:Envelope>
        """;

    private const string RespuestaUltimoAutorizado = """
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
          <s:Body>
            <FECompUltimoAutorizadoResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECompUltimoAutorizadoResult>
                <PtoVta>1</PtoVta>
                <CbteTipo>6</CbteTipo>
                <CbteNro>1</CbteNro>
              </FECompUltimoAutorizadoResult>
            </FECompUltimoAutorizadoResponse>
          </s:Body>
        </s:Envelope>
        """;

    private string PedidoCae => $"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ar="{Namespace}">
          <soapenv:Body>
            <ar:FECAESolicitar>
              <ar:Auth>
                <ar:Token>TU_API_KEY</ar:Token>
                <ar:Sign></ar:Sign>
                <ar:Cuit>{CuitEjemplo}</ar:Cuit>
              </ar:Auth>
              <ar:FeCAEReq>
                <ar:FeCabReq>
                  <ar:CantReg>1</ar:CantReg>
                  <ar:PtoVta>1</ar:PtoVta>
                  <ar:CbteTipo>6</ar:CbteTipo>            <!-- Factura B -->
                </ar:FeCabReq>
                <ar:FeDetReq>
                  <ar:FECAEDetRequest>
                    <ar:Concepto>1</ar:Concepto>          <!-- productos -->
                    <ar:DocTipo>99</ar:DocTipo>           <!-- consumidor final sin identificar -->
                    <ar:DocNro>0</ar:DocNro>
                    <ar:CbteDesde>2</ar:CbteDesde>        <!-- último autorizado + 1 -->
                    <ar:CbteHasta>2</ar:CbteHasta>
                    <ar:CbteFch>{Hoy}</ar:CbteFch>
                    <ar:ImpTotal>121</ar:ImpTotal>
                    <ar:ImpTotConc>0</ar:ImpTotConc>
                    <ar:ImpNeto>100</ar:ImpNeto>
                    <ar:ImpOpEx>0</ar:ImpOpEx>
                    <ar:ImpTrib>0</ar:ImpTrib>
                    <ar:ImpIVA>21</ar:ImpIVA>
                    <ar:MonId>PES</ar:MonId>
                    <ar:MonCotiz>1</ar:MonCotiz>
                    <ar:Iva>
                      <ar:AlicIva>
                        <ar:Id>5</ar:Id>                  <!-- 21 % -->
                        <ar:BaseImp>100</ar:BaseImp>
                        <ar:Importe>21</ar:Importe>
                      </ar:AlicIva>
                    </ar:Iva>
                    <ar:CondicionIVAReceptorId>5</ar:CondicionIVAReceptorId>  <!-- consumidor final -->
                  </ar:FECAEDetRequest>
                </ar:FeDetReq>
              </ar:FeCAEReq>
            </ar:FECAESolicitar>
          </soapenv:Body>
        </soapenv:Envelope>
        """;

    private string RespuestaCaeAprobada => $"""
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
          <s:Body>
            <FECAESolicitarResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECAESolicitarResult>
                <FeCabResp>
                  <Cuit>{CuitEjemplo}</Cuit>
                  <PtoVta>1</PtoVta>
                  <CbteTipo>6</CbteTipo>
                  <FchProceso>{Hoy}203814</FchProceso>
                  <CantReg>1</CantReg>
                  <Resultado>A</Resultado>
                  <Reproceso>N</Reproceso>
                </FeCabResp>
                <FeDetResp>
                  <FECAEDetResponse>
                    <Concepto>1</Concepto>
                    <DocTipo>99</DocTipo>
                    <DocNro>0</DocNro>
                    <CbteDesde>2</CbteDesde>
                    <CbteHasta>2</CbteHasta>
                    <CbteFch>{Hoy}</CbteFch>
                    <Resultado>A</Resultado>
                    <CAE>97570838021736</CAE>
                    <CAEFchVto>{DateTime.Today.AddDays(10).ToString("yyyyMMdd", CultureInfo.InvariantCulture)}</CAEFchVto>
                  </FECAEDetResponse>
                </FeDetResp>
              </FECAESolicitarResult>
            </FECAESolicitarResponse>
          </s:Body>
        </s:Envelope>
        """;

    private string RespuestaCaeRechazada => $"""
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
          <s:Body>
            <FECAESolicitarResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECAESolicitarResult>
                <FeCabResp>
                  <Cuit>{CuitEjemplo}</Cuit>
                  <PtoVta>1</PtoVta>
                  <CbteTipo>6</CbteTipo>
                  <FchProceso>{Hoy}203814</FchProceso>
                  <CantReg>1</CantReg>
                  <Resultado>R</Resultado>
                  <Reproceso>N</Reproceso>
                </FeCabResp>
                <FeDetResp>
                  <FECAEDetResponse>
                    <Concepto>1</Concepto>
                    <DocTipo>99</DocTipo>
                    <DocNro>0</DocNro>
                    <CbteDesde>3</CbteDesde>
                    <CbteHasta>3</CbteHasta>
                    <CbteFch>{Hoy}</CbteFch>
                    <Resultado>R</Resultado>
                    <CAE />
                    <CAEFchVto />
                  </FECAEDetResponse>
                </FeDetResp>
                <Errors>
                  <Err>
                    <Code>400</Code>
                    <Msg>10048: El importe total no coincide con la suma de los importes.</Msg>
                  </Err>
                </Errors>
              </FECAESolicitarResult>
            </FECAESolicitarResponse>
          </s:Body>
        </s:Envelope>
        """;

    private string PedidoConsultar => $"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ar="{Namespace}">
          <soapenv:Body>
            <ar:FECompConsultar>
              <ar:Auth>
                <ar:Token>TU_API_KEY</ar:Token>
                <ar:Sign></ar:Sign>
                <ar:Cuit>{CuitEjemplo}</ar:Cuit>
              </ar:Auth>
              <ar:FeCompConsReq>
                <ar:CbteTipo>6</ar:CbteTipo>
                <ar:CbteNro>2</ar:CbteNro>
                <ar:PtoVta>1</ar:PtoVta>
              </ar:FeCompConsReq>
            </ar:FECompConsultar>
          </soapenv:Body>
        </soapenv:Envelope>
        """;

    private string RespuestaConsultar => $"""
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
          <s:Body>
            <FECompConsultarResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECompConsultarResult>
                <ResultGet>
                  <Concepto>1</Concepto>
                  <DocTipo>99</DocTipo>
                  <DocNro>0</DocNro>
                  <CbteDesde>2</CbteDesde>
                  <CbteHasta>2</CbteHasta>
                  <CbteFch>{Hoy}</CbteFch>
                  <ImpTotal>121</ImpTotal>
                  <ImpTotConc>0</ImpTotConc>
                  <ImpNeto>100</ImpNeto>
                  <ImpOpEx>0</ImpOpEx>
                  <ImpTrib>0</ImpTrib>
                  <ImpIVA>21</ImpIVA>
                  <MonId>PES</MonId>
                  <MonCotiz>1</MonCotiz>
                  <Resultado>A</Resultado>
                  <CodAutorizacion>97570838021736</CodAutorizacion>
                  <EmisionTipo>CAE</EmisionTipo>
                  <FchVto>{DateTime.Today.AddDays(10).ToString("yyyyMMdd", CultureInfo.InvariantCulture)}</FchVto>
                  <PtoVta>1</PtoVta>
                  <CbteTipo>6</CbteTipo>
                </ResultGet>
              </FECompConsultarResult>
            </FECompConsultarResponse>
          </s:Body>
        </s:Envelope>
        """;

    private const string RespuestaErrorAuth = """
        <FECompUltimoAutorizadoResult>
          <PtoVta>1</PtoVta>
          <CbteTipo>6</CbteTipo>
          <CbteNro>0</CbteNro>
          <Errors>
            <Err>
              <Code>600</Code>
              <Msg>La API key es inválida o fue revocada.</Msg>
            </Err>
          </Errors>
        </FECompUltimoAutorizadoResult>
        """;

    // ===== Programas de ejemplo =====

    private string EjemploCurl => $$"""
        # Guardá el pedido (por ejemplo, el de FECAESolicitar de arriba) en pedido.xml y envialo:
        curl -X POST "{{UrlServicio}}" \
          -H "Content-Type: text/xml; charset=utf-8" \
          -H 'SOAPAction: "{{Namespace}}FECAESolicitar"' \
          --data-binary @pedido.xml

        # El SOAPAction cambia con la operación: "{{Namespace}}FECompUltimoAutorizado", etc.
        """;

    private string EjemploPython => $$"""
        # pip install zeep
        import os
        from datetime import date
        from zeep import Client

        WSDL = "{{UrlWsdl}}"
        CUIT = {{CuitEjemplo}}          # CUIT del emisor (la de tu API key)
        PTO_VTA, FACTURA_B = 1, 6

        cliente = Client(WSDL)
        auth = {"Token": os.environ["ARCASIM_API_KEY"], "Sign": "", "Cuit": CUIT}

        def revisar_errores(resultado):
            # Errores de autenticación (600/601) o rechazos (400): vienen en Errors.
            if resultado.Errors:
                raise RuntimeError("; ".join(f"{e.Code}: {e.Msg}" for e in resultado.Errors.Err))

        # 1) Próximo número = último autorizado + 1
        ultimo = cliente.service.FECompUltimoAutorizado(Auth=auth, PtoVta=PTO_VTA, CbteTipo=FACTURA_B)
        revisar_errores(ultimo)
        numero = ultimo.CbteNro + 1

        # 2) Pedir el CAE: Factura B a consumidor final por $121 (neto 100 + IVA 21 %)
        respuesta = cliente.service.FECAESolicitar(Auth=auth, FeCAEReq={
            "FeCabReq": {"CantReg": 1, "PtoVta": PTO_VTA, "CbteTipo": FACTURA_B},
            "FeDetReq": {"FECAEDetRequest": [{
                "Concepto": 1, "DocTipo": 99, "DocNro": 0,
                "CbteDesde": numero, "CbteHasta": numero,
                "CbteFch": date.today().strftime("%Y%m%d"),
                "ImpTotal": 121, "ImpTotConc": 0, "ImpNeto": 100,
                "ImpOpEx": 0, "ImpTrib": 0, "ImpIVA": 21,
                "MonId": "PES", "MonCotiz": 1,
                "Iva": {"AlicIva": [{"Id": 5, "BaseImp": 100, "Importe": 21}]},
                "CondicionIVAReceptorId": 5,
            }]},
        })
        revisar_errores(respuesta)

        detalle = respuesta.FeDetResp.FECAEDetResponse[0]
        print("Resultado:", detalle.Resultado, "| CAE:", detalle.CAE, "| Vence:", detalle.CAEFchVto)
        """;

    private string EjemploNode => $$"""
        // npm install soap
        const soap = require("soap");

        const WSDL = "{{UrlWsdl}}";
        const CUIT = {{CuitEjemplo}};          // CUIT del emisor (la de tu API key)
        const PTO_VTA = 1, FACTURA_B = 6;

        const Auth = { Token: process.env.ARCASIM_API_KEY, Sign: "", Cuit: CUIT };

        // Errores de autenticación (600/601) o rechazos (400): vienen en Errors.
        function revisarErrores(resultado) {
          const errores = resultado.Errors?.Err ?? [];
          if (errores.length) throw new Error(errores.map(e => `${e.Code}: ${e.Msg}`).join("; "));
        }

        async function main() {
          const cliente = await soap.createClientAsync(WSDL);

          // 1) Próximo número = último autorizado + 1
          const [ultimo] = await cliente.FECompUltimoAutorizadoAsync({ Auth, PtoVta: PTO_VTA, CbteTipo: FACTURA_B });
          revisarErrores(ultimo.FECompUltimoAutorizadoResult);
          const numero = ultimo.FECompUltimoAutorizadoResult.CbteNro + 1;

          // 2) Pedir el CAE: Factura B a consumidor final por $121 (neto 100 + IVA 21 %)
          const hoy = new Date().toLocaleDateString("sv-SE", { timeZone: "America/Argentina/Buenos_Aires" }).replaceAll("-", "");
          const [respuesta] = await cliente.FECAESolicitarAsync({
            Auth,
            FeCAEReq: {
              FeCabReq: { CantReg: 1, PtoVta: PTO_VTA, CbteTipo: FACTURA_B },
              FeDetReq: {
                FECAEDetRequest: [{
                  Concepto: 1, DocTipo: 99, DocNro: 0,
                  CbteDesde: numero, CbteHasta: numero, CbteFch: hoy,
                  ImpTotal: 121, ImpTotConc: 0, ImpNeto: 100, ImpOpEx: 0, ImpTrib: 0, ImpIVA: 21,
                  MonId: "PES", MonCotiz: 1,
                  Iva: { AlicIva: [{ Id: 5, BaseImp: 100, Importe: 21 }] },
                  CondicionIVAReceptorId: 5,
                }],
              },
            },
          });
          revisarErrores(respuesta.FECAESolicitarResult);

          const [detalle] = respuesta.FECAESolicitarResult.FeDetResp.FECAEDetResponse;
          console.log(`Resultado: ${detalle.Resultado} | CAE: ${detalle.CAE} | Vence: ${detalle.CAEFchVto}`);
        }

        main().catch(e => { console.error(e.message); process.exitCode = 1; });
        """;

    private string EjemploCSharp => $$"""
        // .NET 8 o superior, sin paquetes extra: el pedido se arma con XDocument y se envía con HttpClient.
        using System.Text;
        using System.Xml.Linq;

        const string Url = "{{UrlServicio}}";
        const long Cuit = {{CuitEjemplo}};          // CUIT del emisor (la de tu API key)
        const int PtoVta = 1, FacturaB = 6;

        XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace ar = "{{Namespace}}";
        string apiKey = Environment.GetEnvironmentVariable("ARCASIM_API_KEY")
            ?? throw new InvalidOperationException("Falta la variable ARCASIM_API_KEY.");

        using var http = new HttpClient();

        XElement Auth() => new(ar + "Auth",
            new XElement(ar + "Token", apiKey),
            new XElement(ar + "Sign", ""),
            new XElement(ar + "Cuit", Cuit));

        XElement E(string nombre, params object[] contenido) => new(ar + nombre, contenido);

        async Task<XElement> LlamarAsync(string operacion, params object[] parametros)
        {
            var sobre = new XDocument(new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "ar", ar),
                new XElement(soapenv + "Body", E(operacion, parametros))));

            using var pedido = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(sobre.ToString(), Encoding.UTF8, "text/xml")
            };
            pedido.Headers.Add("SOAPAction", $"\"{ar.NamespaceName}{operacion}\"");

            using var respuesta = await http.SendAsync(pedido);
            var xml = XDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            var resultado = xml.Descendants(ar + (operacion + "Result")).Single();

            // Errores de autenticación (600/601) o rechazos (400): vienen en <Errors>.
            var errores = resultado.Elements(ar + "Errors").Elements(ar + "Err")
                .Select(e => $"{e.Element(ar + "Code")?.Value}: {e.Element(ar + "Msg")?.Value}")
                .ToList();
            if (errores.Count > 0)
                throw new InvalidOperationException(string.Join("; ", errores));

            return resultado;
        }

        // 1) Próximo número = último autorizado + 1
        var ultimo = await LlamarAsync("FECompUltimoAutorizado", Auth(), E("PtoVta", PtoVta), E("CbteTipo", FacturaB));
        long numero = (long)ultimo.Element(ar + "CbteNro")! + 1;

        // 2) Pedir el CAE: Factura B a consumidor final por $121 (neto 100 + IVA 21 %)
        var cae = await LlamarAsync("FECAESolicitar", Auth(),
            E("FeCAEReq",
                E("FeCabReq", E("CantReg", 1), E("PtoVta", PtoVta), E("CbteTipo", FacturaB)),
                E("FeDetReq", E("FECAEDetRequest",
                    E("Concepto", 1), E("DocTipo", 99), E("DocNro", 0),
                    E("CbteDesde", numero), E("CbteHasta", numero),
                    E("CbteFch", DateTime.Today.ToString("yyyyMMdd")),
                    E("ImpTotal", 121m), E("ImpTotConc", 0), E("ImpNeto", 100m),
                    E("ImpOpEx", 0), E("ImpTrib", 0), E("ImpIVA", 21m),
                    E("MonId", "PES"), E("MonCotiz", 1),
                    E("Iva", E("AlicIva", E("Id", 5), E("BaseImp", 100m), E("Importe", 21m))),
                    E("CondicionIVAReceptorId", 5)))));

        var detalle = cae.Element(ar + "FeDetResp")!.Element(ar + "FECAEDetResponse")!;
        Console.WriteLine($"Resultado: {detalle.Element(ar + "Resultado")?.Value} | " +
            $"CAE: {detalle.Element(ar + "CAE")?.Value} | Vence: {detalle.Element(ar + "CAEFchVto")?.Value}");
        """;
}
