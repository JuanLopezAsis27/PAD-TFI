namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Datos para emitir y autorizar manualmente un comprobante desde el portal web.
/// </summary>
public class EmitirComprobanteRequest
{
    public string CuitEmisor { get; set; } = string.Empty;
    public int PuntoVentaNumero { get; set; } = 1;

    /// <summary>Tipo de comprobante según ARCA (1 = Factura A, 6 = Factura B, 11 = Factura C, etc.).</summary>
    public int TipoComprobante { get; set; } = 1;

    /// <summary>Concepto según ARCA (1 = Productos, 2 = Servicios, 3 = Productos y Servicios).</summary>
    public int Concepto { get; set; } = 1;

    /// <summary>Si es null, se usa el próximo número correlativo disponible.</summary>
    public long? Numero { get; set; }

    /// <summary>Fecha de emisión (si es null, se toma la fecha de hoy).</summary>
    public DateTime? Fecha { get; set; }

    public DateTime? FechaServicioDesde { get; set; }
    public DateTime? FechaServicioHasta { get; set; }
    public DateTime? FechaVencimientoPago { get; set; }

    /// <summary>Tipo de documento del receptor (80 = CUIT, 86 = CUIL, 96 = DNI, 99 = Sin identificar).</summary>
    public int TipoDocumentoReceptor { get; set; } = 80;

    public string NumeroDocumentoReceptor { get; set; } = string.Empty;

    /// <summary>Condición frente al IVA del receptor (RG 5616: 1 = RI, 4 = Exento, 5 = Consumidor Final, 6 = Monotributista).</summary>
    public int CondicionIvaReceptor { get; set; } = 5;

    public decimal ImporteNeto { get; set; }
    public decimal ImporteNoGravado { get; set; }
    public decimal ImporteExento { get; set; }
    public decimal ImporteTributos { get; set; }
    public decimal ImporteIva { get; set; }
    public decimal ImporteTotal { get; set; }

    public string Moneda { get; set; } = "PES";
    public decimal Cotizacion { get; set; } = 1m;
    public bool CancelaEnMismaMonedaExtranjera { get; set; }

    public List<AlicuotaIvaDto> AlicuotasIva { get; set; } = new();
}

public class AlicuotaIvaDto
{
    /// <summary>Código de alícuota ARCA (3 = 0%, 4 = 10.5%, 5 = 21%, 6 = 27%, 8 = 5%, 9 = 2.5%).</summary>
    public int AlicuotaId { get; set; } = 5;
    public decimal BaseImponible { get; set; }
    public decimal ImporteIva { get; set; }
}

