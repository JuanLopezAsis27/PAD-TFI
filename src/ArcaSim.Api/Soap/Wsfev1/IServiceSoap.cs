using System.ServiceModel;
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1;

/// <summary>
/// Contrato SOAP de WSFEv1 (factura electrónica). Debe ser IDÉNTICO al de ARCA
/// (https://wswhomo.afip.gov.ar/wsfev1/service.asmx?WSDL): mismo namespace, mismos nombres de
/// operaciones y de elementos, para que un sistema hecho para ARCA funcione cambiando solo la URL.
/// </summary>
[ServiceContract(Namespace = Ns, Name = "ServiceSoap")]
[XmlSerializerFormat]
public interface IServiceSoap
{
    public const string Ns = "http://ar.gov.afip.dif.FEV1/";

    /// <summary>Verificación de funcionamiento (no requiere autenticación).</summary>
    [OperationContract(Action = Ns + "FEDummy")]
    DummyResponse FEDummy();

    // TODO: agregar el resto de las operaciones con los mismos nombres que en ARCA. Todas reciben
    // el objeto Auth (Token = API key, Sign vacío, Cuit = emisor). Por ejemplo:
    //   FERecuperaLastCbteResponse FECompUltimoAutorizado(FEAuthRequest Auth, int PtoVta, int CbteTipo);
    //   FECAEResponse FECAESolicitar(FEAuthRequest Auth, FECAERequest FeCAEReq);
    //   FECompConsultaResponse FECompConsultar(FEAuthRequest Auth, FECompConsultaReq FeCompConsReq);
    //   FEParamGetTiposCbte, FEParamGetTiposIva, FEParamGetCondicionIvaReceptor, ...
}

[XmlType(Namespace = IServiceSoap.Ns)]
public class DummyResponse
{
    public string? AppServer { get; set; }
    public string? DbServer { get; set; }
    public string? AuthServer { get; set; }
}
