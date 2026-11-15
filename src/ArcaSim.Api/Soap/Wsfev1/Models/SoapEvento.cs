using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(TypeName = "Evt", Namespace = IServiceSoap.Ns)]
public class SoapEvento
{
    [XmlElement(Order = 0)]
    public int Code { get; set; }

    [XmlElement(Order = 1)]
    public string Msg { get; set; } = string.Empty;
}