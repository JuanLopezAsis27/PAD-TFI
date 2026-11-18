
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEParamGetCondicionIvaReceptorResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("CondicionIvaReceptor")]
    public List<FECondicionIvaReceptor>? ResultGet { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}

[XmlType(Namespace = IServiceSoap.Ns)]
public class FECondicionIvaReceptor
{
    [XmlElement(Order = 0)]
    public int Id { get; set; }

    [XmlElement(Order = 1)]
    public string Desc { get; set; } = string.Empty;

    [XmlElement("Cmp_Clase", Order = 2)]
    public string CmpClase { get; set; } = string.Empty;
}
