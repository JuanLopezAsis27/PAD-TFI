using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class MapeadorDatosQrComprobanteTests
{
    private static Comprobante CrearComprobanteAutorizado()
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
                FechaVencimientoCAE = new DateOnly(2026, 11, 7)
            });
        return comprobante;
    }

    [Fact]
    public void Mapear_ComprobanteAutorizado_DevuelveDatosCorrectos()
    {
        var comprobante = CrearComprobanteAutorizado();
        var resultado = MapeadorDatosQrComprobante.Mapear(comprobante);

        Assert.Equal(new DateOnly(2026, 10, 28), resultado.Fecha);
        Assert.Equal("20301234563", resultado.CUITEmisor);
        Assert.Equal(1, resultado.PuntoVenta);
        Assert.Equal((int)TipoComprobante.FacturaB, resultado.TipoComprobante);
        Assert.Equal(42, resultado.NumeroComprobante);
        Assert.Equal(1210m, resultado.ImporteTotal);
        Assert.Equal("PES", resultado.Moneda);
        Assert.Equal(1m, resultado.Cotizacion);
        Assert.Equal((int)TipoDocumento.DNI, resultado.TipoDocumentoReceptor);
        Assert.Equal("12345678", resultado.NumeroDocumentoReceptor);
        Assert.Equal("E", resultado.TipoAutorizacion);
        Assert.Equal("12345678901234", resultado.CodigoAutorizacion);
    }

    [Fact]
    public void Mapear_ComprobanteAutorizadoConObservaciones_DevuelveDatos()
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
                Observaciones = new[] { "OBS001" }
            });
        var resultado = MapeadorDatosQrComprobante.Mapear(comprobante);
        Assert.Equal("12345678901234", resultado.CodigoAutorizacion);
    }

    [Fact]
    public void Mapear_ComprobantePendiente_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        Assert.Throws<InvalidOperationException>(() => MapeadorDatosQrComprobante.Mapear(comprobante));
    }

    [Fact]
    public void Mapear_ComprobanteRechazado_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        comprobante.AplicarResultadoAutorizacion(
            new ResultadoAutorizacion
            {
                Autorizado = false
            });
        Assert.Throws<InvalidOperationException>(() => MapeadorDatosQrComprobante.Mapear(comprobante));
    }

    [Fact]
    public void Mapear_CaeVacio_LanzaExcepcion()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.CAE = string.Empty;
        Assert.Throws<InvalidOperationException>(() => MapeadorDatosQrComprobante.Mapear(comprobante));
    }

    [Fact]
    public void Mapear_SinFechaVencimientoCae_LanzaExcepcion()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.FechaVencimientoCAE = null;
        Assert.Throws<InvalidOperationException>(() => MapeadorDatosQrComprobante.Mapear(comprobante));
    }

    [Fact]
    public void Mapear_ComprobanteNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => MapeadorDatosQrComprobante.Mapear(null!));
    }

    [Fact]
    public void Mapear_SinCotizacion_LanzaExcepcion()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.Cotizacion = null;
        Assert.Throws<InvalidOperationException>(() => MapeadorDatosQrComprobante.Mapear(comprobante));
    }

    [Fact]
    public void Mapear_ComprobanteSinDocumentoReceptor_OmiteDatosDelDocumento()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.TipoDocumentoReceptor = default;
        comprobante.NumeroDocumentoReceptor = string.Empty;
        var resultado = MapeadorDatosQrComprobante.Mapear(comprobante);
        Assert.Null(resultado.TipoDocumentoReceptor);
        Assert.Null(resultado.NumeroDocumentoReceptor);
    }

    [Fact]
    public void Mapear_ComprobanteConDocumentoVacio_OmiteDatosDelDocumento()
    {
        var comprobante = CrearComprobanteAutorizado();
        comprobante.NumeroDocumentoReceptor = string.Empty;
        var resultado = MapeadorDatosQrComprobante.Mapear(comprobante);
        Assert.Null(resultado.TipoDocumentoReceptor);
        Assert.Null(resultado.NumeroDocumentoReceptor);
    }


}