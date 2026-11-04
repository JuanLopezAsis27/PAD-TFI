using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Entidades;

public class Comprobante
{
    public string CUITEmisor { get; set; } = string.Empty;
    public ConceptoComprobante Concepto { get; set; }
    public int PuntoVentaNumero { get; set; }
    public TipoComprobante Tipo { get; set; }
    public long Numero { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? FechaServicioDesde { get; set; }
    public DateTime? FechaServicioHasta { get; set; }
    public DateTime? FechaVencimientoPago { get; set; }
    public TipoDocumento TipoDocumentoReceptor { get; set; }
    public string NumeroDocumentoReceptor { get; set; } = string.Empty;
    public CondicionIva? CondicionIvaReceptor { get; set; }
    public decimal ImporteNeto { get; set; }
    public decimal ImporteNoGravado { get; set; }
    public decimal ImporteExento { get; set; }
    public decimal ImporteTributos { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Moneda { get; set; } = "PES";
    public bool CancelaEnMismaMonedaExtranjera { get; set; }
    public decimal? Cotizacion { get; set; } = 1m;
    public string? CAE { get; set; }
    public DateTime? FechaVencimientoCAE { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public List<ComprobanteIva> AlicuotasIva { get; set; } = new();
    public EstadoAutorizacionComprobante EstadoAutorizacion { get; private set; }
    = EstadoAutorizacionComprobante.Pendiente;
    public IReadOnlyList<string> ObservacionesAutorizacion { get; private set; }
    = Array.Empty<string>();

    public void AplicarResultadoAutorizacion(ResultadoAutorizacion resultado)
    {
        ArgumentNullException.ThrowIfNull(resultado);

        if (EstadoAutorizacion != EstadoAutorizacionComprobante.Pendiente)
            throw new InvalidOperationException("El comprobante ya tiene un resultado de autorización.");

        if (!ValidadorResultadoAutorizacion.EsValido(resultado))
            throw new ArgumentException("El resultado de autorización no es válido.", nameof(resultado));

        ObservacionesAutorizacion = resultado.Observaciones.ToArray();

        if (!resultado.Autorizado)
        {
            CAE = null;
            FechaVencimientoCAE = null;
            EstadoAutorizacion = EstadoAutorizacionComprobante.Rechazado;
            return;
        }

        CAE = resultado.CAE;
        FechaVencimientoCAE = resultado.FechaVencimientoCAE.ToDateTime(TimeOnly.MinValue);

        EstadoAutorizacion = resultado.Observaciones.Count > 0
            ? EstadoAutorizacionComprobante.AutorizadoConObservaciones
            : EstadoAutorizacionComprobante.Autorizado;
    }
}