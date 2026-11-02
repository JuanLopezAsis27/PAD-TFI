using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ServicioQrComprobanteTests
{
    private static Comprobante CrearComprobanteAutorizado(string[]? observaciones = null)
    {
        var comprobante = new Comprobante
        {
            CUITEmisor = "20301234563",
            Fecha = new DateTime(2026, 10, 28),
            PuntoVentaNumero = 1,
            Tipo = TipoComprobante.FacturaB,
            Numero = 42,
            ImporteTotal = 1210m,
            Moneda = "PES",
            Cotizacion = 1m,
            TipoDocumentoReceptor = TipoDocumento.DNI,
            NumeroDocumentoReceptor = "12345678"
        };
        comprobante.AplicarResultadoAutorizacion(
            new ResultadoAutorizacion
            {
                Autorizado = true,
                CAE = "12345678901234",
                FechaVencimientoCAE = new DateOnly(2026, 11, 7),
                Observaciones = observaciones ?? Array.Empty<string>()
            });
        return comprobante;
    }

    [Fact]
    public void GenerarUrl_ComprobanteAutorizado_DevuelveUrlDeArca()
    {
        var comprobante = CrearComprobanteAutorizado();
        var url = ServicioQrComprobante.GenerarUrl(comprobante);
        Assert.StartsWith("https://www.arca.gob.ar/fe/qr/?p=", url);
    }

    [Fact]
    public void GenerarUrl_ComprobanteAutorizadoConObservaciones_DevuelveUrl()
    {
        var comprobante = CrearComprobanteAutorizado(new[] { "OBS001" });
        var url = ServicioQrComprobante.GenerarUrl(comprobante);
        Assert.StartsWith("https://www.arca.gob.ar/fe/qr/?p=", url);
    }

    [Fact]
    public void GenerarUrl_ComprobantePendiente_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        Assert.Throws<InvalidOperationException>(() => ServicioQrComprobante.GenerarUrl(comprobante));
    }

    [Fact]
    public void GenerarUrl_ComprobanteRechazado_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        comprobante.AplicarResultadoAutorizacion(
            new ResultadoAutorizacion
            {
                Autorizado = false
            });
        Assert.Throws<InvalidOperationException>(() => ServicioQrComprobante.GenerarUrl(comprobante));
    }

    [Fact]
    public void GenerarUrl_ComprobanteConDatosInvalidos_LanzaExcepcion()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.PuntoVentaNumero = 0;
        Assert.Throws<ArgumentException>(() => ServicioQrComprobante.GenerarUrl(comprobante));
    }

    [Fact]
    public void GenerarUrl_ComprobanteNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => ServicioQrComprobante.GenerarUrl(null!));
    }
}