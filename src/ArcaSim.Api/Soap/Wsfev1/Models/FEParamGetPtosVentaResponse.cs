
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// RESPUESTA DE FEParamGetPtosVenta
// =====================================================

[XmlType(
    TypeName = "FEParamGetPtosVentaResponse",
    Namespace = IServiceSoap.Ns)]
public class FEParamGetPtosVentaResponse
{
    [XmlArray("ResultGet", Order = 0)]
    [XmlArrayItem("PtoVenta")]
    public List<FEPtoVenta>? ResultGet { get; set; }

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
// PUNTO DE VENTA
// =====================================================

[XmlType(
    TypeName = "PtoVenta",
    Namespace = IServiceSoap.Ns)]
public class FEPtoVenta
{
    [XmlElement(Order = 0)]
    public int Nro { get; set; }

    [XmlElement(Order = 1)]
    public string EmisionTipo { get; set; } = "CAE";

    [XmlElement(Order = 2)]
    public string Bloqueado { get; set; } = "N";

    [XmlElement(Order = 3)]
    public string? FchBaja { get; set; }

    public bool ShouldSerializeFchBaja() =>
        !string.IsNullOrWhiteSpace(FchBaja);
}
