
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// FEParamGetTiposCbte
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEParamGetTiposCbteResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("CbteTipo")]
    public List<FECbteTipo>? ResultGet { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}

[XmlType(Namespace = IServiceSoap.Ns)]
public class FECbteTipo
{
    [XmlElement(Order = 0)]
    public int Id { get; set; }

    [XmlElement(Order = 1)]
    public string Desc { get; set; } = string.Empty;

    [XmlElement(Order = 2)]
    public string? FchDesde { get; set; }

    public bool ShouldSerializeFchDesde() =>
        !string.IsNullOrWhiteSpace(FchDesde);

    [XmlElement(Order = 3)]
    public string? FchHasta { get; set; }

    public bool ShouldSerializeFchHasta() =>
        !string.IsNullOrWhiteSpace(FchHasta);
}

// =====================================================
// FEParamGetTiposIva
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEParamGetTiposIvaResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("IvaTipo")]
    public List<FEIvaTipo>? ResultGet { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEIvaTipo
{
    [XmlElement(Order = 0)]
    public string Id { get; set; } = string.Empty;

    [XmlElement(Order = 1)]
    public string Desc { get; set; } = string.Empty;

    [XmlElement(Order = 2)]
    public string? FchDesde { get; set; }

    public bool ShouldSerializeFchDesde() =>
        !string.IsNullOrWhiteSpace(FchDesde);

    [XmlElement(Order = 3)]
    public string? FchHasta { get; set; }

    public bool ShouldSerializeFchHasta() =>
        !string.IsNullOrWhiteSpace(FchHasta);
}
