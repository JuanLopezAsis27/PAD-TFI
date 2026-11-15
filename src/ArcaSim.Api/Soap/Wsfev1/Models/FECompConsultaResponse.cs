
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// RESPUESTA PRINCIPAL
// =====================================================

[XmlType(
    TypeName = "FECompConsultaResponse",
    Namespace = IServiceSoap.Ns)]
public class FECompConsultaResponse
{
    [XmlElement(Order = 0)]
    public FECompConsultaResult? ResultGet { get; set; }

    [XmlArray("Errors", Order = 1)]
    [XmlArrayItem("Err")]
    public List<SoapError>? Errors { get; set; }

    [XmlArray("Events", Order = 2)]
    [XmlArrayItem("Evt")]
    public List<SoapEvento>? Events { get; set; }
}

// =====================================================
// RESULTADO DE LA CONSULTA
// =====================================================

[XmlType(
    TypeName = "FECompConsultaResult",
    Namespace = IServiceSoap.Ns)]
public class FECompConsultaResult
{
    [XmlElement(Order = 0)]
    public int Concepto { get; set; }

    [XmlElement(Order = 1)]
    public int DocTipo { get; set; }

    [XmlElement(Order = 2)]
    public long DocNro { get; set; }

    [XmlElement(Order = 3)]
    public long CbteDesde { get; set; }

    [XmlElement(Order = 4)]
    public long CbteHasta { get; set; }

    [XmlElement(Order = 5)]
    public string CbteFch { get; set; } = string.Empty;

    [XmlElement(Order = 6)]
    public double ImpTotal { get; set; }

    [XmlElement(Order = 7)]
    public double ImpTotConc { get; set; }

    [XmlElement(Order = 8)]
    public double ImpNeto { get; set; }

    [XmlElement(Order = 9)]
    public double ImpOpEx { get; set; }

    [XmlElement(Order = 10)]
    public double ImpTrib { get; set; }

    [XmlElement(Order = 11)]
    public double ImpIVA { get; set; }

    // =================================================
    // FECHAS OPCIONALES
    // =================================================

    [XmlElement(Order = 12)]
    public string? FchServDesde { get; set; }

    public bool ShouldSerializeFchServDesde() =>
        !string.IsNullOrWhiteSpace(FchServDesde);

    [XmlElement(Order = 13)]
    public string? FchServHasta { get; set; }

    public bool ShouldSerializeFchServHasta() =>
        !string.IsNullOrWhiteSpace(FchServHasta);

    [XmlElement(Order = 14)]
    public string? FchVtoPago { get; set; }

    public bool ShouldSerializeFchVtoPago() =>
        !string.IsNullOrWhiteSpace(FchVtoPago);

    // =================================================
    // MONEDA
    // =================================================

    [XmlElement(Order = 15)]
    public string MonId { get; set; } = "PES";

    [XmlElement(Order = 16)]
    public double MonCotiz { get; set; } = 1;

    // =================================================
    // COLECCIONES OPCIONALES
    // =================================================

    [XmlArray("CbtesAsoc", Order = 17)]
    [XmlArrayItem("CbteAsoc")]
    public List<FEComprobanteAsociado>? CbtesAsoc { get; set; }

    public bool ShouldSerializeCbtesAsoc() =>
        CbtesAsoc is { Count: > 0 };

    [XmlArray("Tributos", Order = 18)]
    [XmlArrayItem("Tributo")]
    public List<FETributo>? Tributos { get; set; }

    public bool ShouldSerializeTributos() =>
        Tributos is { Count: > 0 };

    [XmlArray("Iva", Order = 19)]
    [XmlArrayItem("AlicIva")]
    public List<FEAlicIva>? Iva { get; set; }

    public bool ShouldSerializeIva() =>
        Iva is { Count: > 0 };

    [XmlArray("Opcionales", Order = 20)]
    [XmlArrayItem("Opcional")]
    public List<FEOpcional>? Opcionales { get; set; }

    public bool ShouldSerializeOpcionales() =>
        Opcionales is { Count: > 0 };

    // =================================================
    // AUTORIZACIÓN
    // =================================================

    [XmlElement(Order = 21)]
    public string Resultado { get; set; } = string.Empty;

    [XmlElement(Order = 22)]
    public string CodAutorizacion { get; set; } = string.Empty;

    [XmlElement(Order = 23)]
    public string EmisionTipo { get; set; } = string.Empty;

    [XmlElement(Order = 24)]
    public string FchVto { get; set; } = string.Empty;

    [XmlElement(Order = 25)]
    public string? FchProceso { get; set; }

    public bool ShouldSerializeFchProceso() =>
        !string.IsNullOrWhiteSpace(FchProceso);

    // =================================================
    // OBSERVACIONES
    // =================================================

    [XmlArray("Observaciones", Order = 26)]
    [XmlArrayItem("Obs")]
    public List<SoapObservacion>? Observaciones { get; set; }

    // =================================================
    // IDENTIFICACIÓN DEL COMPROBANTE
    // =================================================

    [XmlElement(Order = 27)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 28)]
    public int CbteTipo { get; set; }
}

// =====================================================
// OBSERVACIÓN SOAP
// =====================================================

[XmlType(
    TypeName = "Obs",
    Namespace = IServiceSoap.Ns)]
public class SoapObservacion
{
    [XmlElement(Order = 0)]
    public int Code { get; set; }

    [XmlElement(Order = 1)]
    public string Msg { get; set; } = string.Empty;
}
