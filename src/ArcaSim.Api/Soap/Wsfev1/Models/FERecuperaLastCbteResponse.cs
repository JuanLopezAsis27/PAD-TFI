
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(
    TypeName = "FERecuperaLastCbteResponse",
    Namespace = IServiceSoap.Ns)]
public class FERecuperaLastCbteResponse
{
    [XmlElement(Order = 0)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 1)]
    public int CbteTipo { get; set; }

    [XmlElement(Order = 2)]
    public int CbteNro { get; set; }

    [XmlArray("Errors", Order = 3)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 4)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}
