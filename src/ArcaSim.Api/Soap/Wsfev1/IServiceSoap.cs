
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.ServiceModel;
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1;

[ServiceContract(Namespace = Ns, Name = "ServiceSoap")]
[XmlSerializerFormat]
public interface IServiceSoap
{
    public const string Ns = "http://ar.gov.afip.dif.FEV1/";

    [OperationContract(Action = Ns + "FEDummy")]
    DummyResponse FEDummy();

    [OperationContract(Action = Ns + "FECompUltimoAutorizado")]
    FERecuperaLastCbteResponse FECompUltimoAutorizado(
        FEAuthRequest Auth,
        int PtoVta,
        int CbteTipo);

    [OperationContract(Action = Ns + "FECompConsultar")]
    FECompConsultaResponse FECompConsultar(
        FEAuthRequest Auth,
        FECompConsultaReq FeCompConsReq);

    [OperationContract(Action = Ns + "FECAESolicitar")]
    FECAEResponse FECAESolicitar(
        FEAuthRequest Auth,
        FECAERequest FeCAEReq);

    [OperationContract(Action = Ns + "FEParamGetTiposCbte")]
    FEParamGetTiposCbteResponse FEParamGetTiposCbte(
        FEAuthRequest Auth);

    [OperationContract(Action = Ns + "FEParamGetTiposIva")]
    FEParamGetTiposIvaResponse FEParamGetTiposIva(
        FEAuthRequest Auth);

    [OperationContract(
        Action = Ns + "FEParamGetCondicionIvaReceptor")]
    FEParamGetCondicionIvaReceptorResponse
        FEParamGetCondicionIvaReceptor(
            FEAuthRequest Auth,
            string ClaseCmp);
}

[XmlType(Namespace = IServiceSoap.Ns)]
public class DummyResponse
{
    public string? AppServer { get; set; }
    public string? DbServer { get; set; }
    public string? AuthServer { get; set; }
}
