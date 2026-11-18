
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// RESPUESTA DE FEParamGetTiposDoc
// =====================================================

[XmlType(
    TypeName = "FEParamGetTiposDocResponse",
    Namespace = IServiceSoap.Ns)]
public class FEParamGetTiposDocResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("DocTipo")]
    public List<FEDocTipo>? ResultGet { get; set; }

    public bool ShouldSerializeResultGet() =>
        ResultGet is { Count: > 0 };

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

// =====================================================
// TIPO DE DOCUMENTO
// =====================================================

[XmlType(
    TypeName = "DocTipo",
    Namespace = IServiceSoap.Ns)]
public class FEDocTipo
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
