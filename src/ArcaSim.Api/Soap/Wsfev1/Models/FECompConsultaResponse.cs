
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(
    TypeName = "FECompConsultaResponse",
    Namespace = IServiceSoap.Ns)]
public class FECompConsultaResponse
{
    [XmlElement(Order = 0)]
    public FECompConsultaResult? ResultGet { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}

[XmlType(
    TypeName = "FECompConsultaResult",
    Namespace = IServiceSoap.Ns)]
public class FECompConsultaResult
{
    [XmlElement(Order = 0)]
    public string Resultado { get; set; } = string.Empty;

    [XmlElement(Order = 1)]
    public string CodAutorizacion { get; set; } = string.Empty;

    [XmlElement(Order = 2)]
    public string EmisionTipo { get; set; } = string.Empty;

    [XmlElement(Order = 3)]
    public string FchVto { get; set; } = string.Empty;

    [XmlElement(Order = 4)]
    public string FchProceso { get; set; } = string.Empty;

    [XmlArray("Observaciones", Order = 5)]
    [XmlArrayItem("Obs")]
    public List<SoapObservacion>? Observaciones { get; set; }

    [XmlElement(Order = 6)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 7)]
    public int CbteTipo { get; set; }
}

[XmlType(TypeName = "Obs", Namespace = IServiceSoap.Ns)]
public class SoapObservacion
{
    [XmlElement(Order = 0)]
    public int Code { get; set; }

    [XmlElement(Order = 1)]
    public string Msg { get; set; } = string.Empty;
}
