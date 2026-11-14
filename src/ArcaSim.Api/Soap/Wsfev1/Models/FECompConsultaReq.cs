using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(
    TypeName = "FECompConsultaReq",
    Namespace = IServiceSoap.Ns)]
public class FECompConsultaReq
{
    [XmlElement(Order = 0)]
    public int CbteTipo { get; set; }

    [XmlElement(Order = 1)]
    public long CbteNro { get; set; }

    [XmlElement(Order = 2)]
    public int PtoVta { get; set; }
}
