
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlRoot(
    ElementName = "FEAuthRequest",
    Namespace = IServiceSoap.Ns)]
[XmlType(
    TypeName = "FEAuthRequest",
    Namespace = IServiceSoap.Ns)]
public class FEAuthRequest
{
    [XmlElement(Order = 0)]
    public string Token { get; set; } = string.Empty;

    [XmlElement(Order = 1)]
    public string Sign { get; set; } = string.Empty;

    [XmlElement(Order = 2)]
    public long Cuit { get; set; }
}
