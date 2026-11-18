
using System.Xml.Serialization;

namespace ArcaSim.Api.Soap.Wsfev1.Models;

// =====================================================
// SOLICITUD PRINCIPAL
// =====================================================

[XmlType(
    TypeName = "FECAERequest",
    Namespace = IServiceSoap.Ns)]
public class FECAERequest
{
    [XmlElement(Order = 0)]
    public FECAECabRequest FeCabReq { get; set; } = new();

    [XmlArray("FeDetReq", Order = 1)]
    [XmlArrayItem("FECAEDetRequest")]
    public List<FECAEDetRequest> FeDetReq { get; set; } = new();
}

// =====================================================
// CABECERA
// =====================================================

[XmlType(
    TypeName = "FECAECabRequest",
    Namespace = IServiceSoap.Ns)]
public class FECAECabRequest
{
    [XmlElement(Order = 0)]
    public int CantReg { get; set; }

    [XmlElement(Order = 1)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 2)]
    public int CbteTipo { get; set; }
}

// =====================================================
// DETALLE DEL COMPROBANTE
// =====================================================

[XmlType(
    TypeName = "FECAEDetRequest",
    Namespace = IServiceSoap.Ns)]
public class FECAEDetRequest
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

    // Fechas opcionales para servicios.

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

    // Moneda y cotización.

    [XmlElement(Order = 15)]
    public string MonId { get; set; } = "PES";

    [XmlElement(Order = 16)]
    public double MonCotiz { get; set; } = 1;

    // Comprobantes asociados.

    [XmlArray("CbtesAsoc", Order = 17)]
    [XmlArrayItem("CbteAsoc")]
    public List<FEComprobanteAsociado>? CbtesAsoc { get; set; }

    public bool ShouldSerializeCbtesAsoc() =>
        CbtesAsoc is { Count: > 0 };

    // Tributos.

    [XmlArray("Tributos", Order = 18)]
    [XmlArrayItem("Tributo")]
    public List<FETributo>? Tributos { get; set; }

    public bool ShouldSerializeTributos() =>
        Tributos is { Count: > 0 };

    // Alícuotas de IVA.

    [XmlArray("Iva", Order = 19)]
    [XmlArrayItem("AlicIva")]
    public List<FEAlicIva>? Iva { get; set; }

    public bool ShouldSerializeIva() =>
        Iva is { Count: > 0 };

    // Campos opcionales.

    [XmlArray("Opcionales", Order = 20)]
    [XmlArrayItem("Opcional")]
    public List<FEOpcional>? Opcionales { get; set; }

    public bool ShouldSerializeOpcionales() =>
        Opcionales is { Count: > 0 };

    // Cancelación en moneda extranjera.

    [XmlElement(Order = 21)]
    public string? CanMisMonExt { get; set; }

    public bool ShouldSerializeCanMisMonExt() =>
        !string.IsNullOrWhiteSpace(CanMisMonExt);

    // Condición IVA del receptor.
    // CORRECCIÓN: se eliminó IsNullable = false.

    [XmlElement(Order = 22)]
    public int? CondicionIVAReceptorId { get; set; }

    public bool ShouldSerializeCondicionIVAReceptorId() =>
        CondicionIVAReceptorId.HasValue;
}

// =====================================================
// COMPROBANTES ASOCIADOS
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEComprobanteAsociado
{
    [XmlElement(Order = 0)]
    public short Tipo { get; set; }

    [XmlElement(Order = 1)]
    public int PtoVta { get; set; }

    [XmlElement(Order = 2)]
    public long Nro { get; set; }

    [XmlElement(Order = 3)]
    public string? Cuit { get; set; }

    public bool ShouldSerializeCuit() =>
        !string.IsNullOrWhiteSpace(Cuit);

    [XmlElement(Order = 4)]
    public string? CbteFch { get; set; }

    public bool ShouldSerializeCbteFch() =>
        !string.IsNullOrWhiteSpace(CbteFch);
}

// =====================================================
// TRIBUTOS
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FETributo
{
    [XmlElement(Order = 0)]
    public short Id { get; set; }

    [XmlElement(Order = 1)]
    public string Desc { get; set; } = string.Empty;

    [XmlElement(Order = 2)]
    public double BaseImp { get; set; }

    [XmlElement(Order = 3)]
    public double Alic { get; set; }

    [XmlElement(Order = 4)]
    public double Importe { get; set; }
}

// =====================================================
// ALÍCUOTAS DE IVA
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEAlicIva
{
    [XmlElement(Order = 0)]
    public short Id { get; set; }

    [XmlElement(Order = 1)]
    public double BaseImp { get; set; }

    [XmlElement(Order = 2)]
    public double Importe { get; set; }
}

// =====================================================
// CAMPOS OPCIONALES
// =====================================================

[XmlType(Namespace = IServiceSoap.Ns)]
public class FEOpcional
{
    [XmlElement(Order = 0)]
    public string Id { get; set; } = string.Empty;

    [XmlElement(Order = 1)]
    public string Valor { get; set; } = string.Empty;
}
