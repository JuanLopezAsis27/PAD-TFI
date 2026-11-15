
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.ServiceModel;
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1;

[ServiceContract(Namespace = Ns, Name = "ServiceSoap")]
[XmlSerializerFormat]
public partial interface IServiceSoap
{
    public const string Ns = "http://ar.gov.afip.dif.FEV1/";

    // =====================================================
    // ESTADO DEL SERVICIO
    // =====================================================

    [OperationContract(Action = Ns + "FEDummy")]
    DummyResponse FEDummy();

    // =====================================================
    // �LTIMO COMPROBANTE AUTORIZADO
    // =====================================================

    [OperationContract(Action = Ns + "FECompUltimoAutorizado")]
    FERecuperaLastCbteResponse FECompUltimoAutorizado(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth,
        [MessageParameter(Name = "PtoVta")]
        int PtoVta,
        [MessageParameter(Name = "CbteTipo")]
        int CbteTipo);

    // =====================================================
    // CONSULTAR COMPROBANTE
    // =====================================================

    [OperationContract(Action = Ns + "FECompConsultar")]
    FECompConsultaResponse FECompConsultar(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth,
        [MessageParameter(Name = "FeCompConsReq")]
        FECompConsultaReq FeCompConsReq);

    // =====================================================
    // SOLICITAR CAE
    // =====================================================

    [OperationContract(Action = Ns + "FECAESolicitar")]
    FECAEResponse FECAESolicitar(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth,
        [MessageParameter(Name = "FeCAEReq")]
        FECAERequest FeCAEReq);

    // =====================================================
    // TIPOS DE COMPROBANTES
    // =====================================================

    [OperationContract(Action = Ns + "FEParamGetTiposCbte")]
    FEParamGetTiposCbteResponse FEParamGetTiposCbte(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth);

    // =====================================================
    // TIPOS DE IVA
    // =====================================================

    [OperationContract(Action = Ns + "FEParamGetTiposIva")]
    FEParamGetTiposIvaResponse FEParamGetTiposIva(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth);

    // =====================================================
    // CONDICI�N IVA DEL RECEPTOR
    // =====================================================

    [OperationContract(
        Action = Ns + "FEParamGetCondicionIvaReceptor")]
    FEParamGetCondicionIvaReceptorResponse
        FEParamGetCondicionIvaReceptor(
            [MessageParameter(Name = "Auth")]
            FEAuthRequest Auth,
            [MessageParameter(Name = "ClaseCmp")]
            string ClaseCmp);
}

// =====================================================
// RESPUESTA FEDummy
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class DummyResponse
{
    public string? AppServer { get; set; }

    public string? DbServer { get; set; }

    public string? AuthServer { get; set; }
}
