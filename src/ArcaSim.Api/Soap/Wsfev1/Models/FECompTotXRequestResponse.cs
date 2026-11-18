
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(
    TypeName = "FERegXReqResponse",
    Namespace = IServiceSoap.Ns)]
public class FERegXReqResponse
{
    [XmlElement(Order = 0)]
    public int RegXReq { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    public bool ShouldSerializeErrors() =>
        Errors is { Count: > 0 };

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }

    public bool ShouldSerializeEvents() =>
        Events is { Count: > 0 };
}
