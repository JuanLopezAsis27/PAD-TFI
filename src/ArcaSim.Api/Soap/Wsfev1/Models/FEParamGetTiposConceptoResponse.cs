
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// RESPUESTA DE FEParamGetTiposConcepto
// =====================================================

[XmlType(
    TypeName = "FEParamGetTiposConceptoResponse",
    Namespace = IServiceSoap.Ns)]
public class FEParamGetTiposConceptoResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("ConceptoTipo")]
    public List<FEConceptoTipo>? ResultGet { get; set; }

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
// TIPO DE CONCEPTO
// =====================================================

[XmlType(
    TypeName = "ConceptoTipo",
    Namespace = IServiceSoap.Ns)]
public class FEConceptoTipo
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
