
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// RESPUESTA PRINCIPAL DE FECAESolicitar
// =====================================================

[XmlType(
    TypeName = "FECAEResponse",
    Namespace = IServiceSoap.Ns)]
public class FECAEResponse
{
    [XmlElement(Order = 0)]
    public FECAECabResponse? FeCabResp { get; set; }

    [XmlArray("FeDetResp", Order = 1)]
    [XmlArrayItem("FECAEDetResponse")]
    public List<FECAEDetResponse>? FeDetResp { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }

    [XmlArray("Errors", Order = 3)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }
}

// =====================================================
// CABECERA DE RESPUESTA
// =====================================================

[XmlType(
    TypeName = "FECAECabResponse",
    Namespace = IServiceSoap.Ns)]
public class FECAECabResponse
{
    [XmlElement(Order = 0)]
    public long Cuit { get; set; }

    [XmlElement(Order = 1)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 2)]
    public int CbteTipo { get; set; }

    [XmlElement(Order = 3)]
    public string FchProceso { get; set; } = string.Empty;

    [XmlElement(Order = 4)]
    public int CantReg { get; set; }

    [XmlElement(Order = 5)]
    public string Resultado { get; set; } = string.Empty;

    [XmlElement(Order = 6)]
    public string Reproceso { get; set; } = "N";
}

// =====================================================
// DETALLE DE RESPUESTA DE CADA COMPROBANTE
// =====================================================

[XmlType(
    TypeName = "FECAEDetResponse",
    Namespace = IServiceSoap.Ns)]
public class FECAEDetResponse
{
    [XmlElement(Order = 0)]
    public int Concepto { get; set; }

    [XmlElement(Order = 1)]
    public int DocTipo { get; set; }

    [XmlElement(Order = 2)]
    public long DocNro { get; set; }

    [XmlElement(Order = 3)]
    public long CbteDesde { get; set; }

    [XmlElement(Order = 4)]
    public long CbteHasta { get; set; }

    [XmlElement(Order = 5)]
    public string CbteFch { get; set; } = string.Empty;

    [XmlElement(Order = 6)]
    public string Resultado { get; set; } = string.Empty;

    [XmlElement(Order = 7)]
    public string CAE { get; set; } = string.Empty;

    [XmlElement(Order = 8)]
    public string CAEFchVto { get; set; } = string.Empty;

    [XmlArray("Observaciones", Order = 9)]
    [XmlArrayItem("Obs")]
    public List<SoapObservacion>? Observaciones { get; set; }
}
