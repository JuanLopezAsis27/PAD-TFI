using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Entidades;

public sealed record class DatosQrComprobante
{
    public DateOnly Fecha { get; init; }
    public string CUITEmisor { get; init; } = string.Empty;
    public int PuntoVenta { get; init; }
    public int TipoComprobante { get; init; }
    public long NumeroComprobante { get; init; }
    public decimal ImporteTotal { get; init; }
    public string Moneda { get; init; } = "PES";
    public decimal Cotizacion { get; init; } = 1m;
    public int? TipoDocumentoReceptor { get; init; }
    public string? NumeroDocumentoReceptor { get; init; }
    public string TipoAutorizacion { get; init; } = "E";
    public string CodigoAutorizacion { get; init; } = string.Empty;
}