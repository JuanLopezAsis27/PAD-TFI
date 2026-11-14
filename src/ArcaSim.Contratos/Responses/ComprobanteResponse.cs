using ArcaSim.Contratos.Requests;

namespace ArcaSim.Contratos.Responses;

public class ComprobanteResponse
{
    public string CuitEmisor { get; set; } = string.Empty;
    public int PuntoVentaNumero { get; set; }
    public int TipoComprobante { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public long Numero { get; set; }
    public int Concepto { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? FechaServicioDesde { get; set; }
    public DateTime? FechaServicioHasta { get; set; }
    public DateTime? FechaVencimientoPago { get; set; }

    public int TipoDocumentoReceptor { get; set; }
    public string NumeroDocumentoReceptor { get; set; } = string.Empty;
    public int? CondicionIvaReceptor { get; set; }

    public decimal ImporteNeto { get; set; }
    public decimal ImporteNoGravado { get; set; }
    public decimal ImporteExento { get; set; }
    public decimal ImporteTributos { get; set; }
    public decimal ImporteIva { get; set; }
    public decimal ImporteTotal { get; set; }

    public string Moneda { get; set; } = "PES";
    public decimal? Cotizacion { get; set; } = 1m;
    public bool CancelaEnMismaMonedaExtranjera { get; set; }

    public string? Cae { get; set; }
    public DateTime? FechaVencimientoCae { get; set; }
    public string EstadoAutorizacion { get; set; } = string.Empty;
    public List<string> Observaciones { get; set; } = new();

    /// <summary>URL oficial de ARCA para el código QR (cuando el comprobante está autorizado).</summary>
    public string? UrlQr { get; set; }

    public List<AlicuotaIvaDto> AlicuotasIva { get; set; } = new();
    public List<ErrorValidacionDto> Errores { get; set; } = new();
}

public class ErrorValidacionDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

