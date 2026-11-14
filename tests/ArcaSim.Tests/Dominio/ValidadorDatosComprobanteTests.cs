using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorDatosComprobanteTests
{
    private static Comprobante CrearComprobanteValido()
    {
        return new Comprobante
        {
            CUITEmisor = "20301234563",
            PuntoVentaNumero = 1,
            Tipo = TipoComprobante.FacturaA,
            Numero = 1,
            Fecha = new DateTime(2026, 10, 28),
            Concepto = ConceptoComprobante.Productos,
            TipoDocumentoReceptor = TipoDocumento.CUIT,
            NumeroDocumentoReceptor = "27301234568",
            CondicionIvaReceptor = CondicionIva.ResponsableInscripto,
            Moneda = "PES",
            Cotizacion = 1m
        };
    }

    [Fact]
    public void EsValido_DatosCorrectos_DevuelveTrue()
    {
        var comprobante = CrearComprobanteValido();
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValido_ReceptorSinIdentificarConNumeroCero_DevuelveTrue()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaB;
        comprobante.TipoDocumentoReceptor = TipoDocumento.SinIdentificar;
        comprobante.NumeroDocumentoReceptor = "0";
        Assert.True(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Theory]
    [InlineData("30123456")]
    [InlineData("00")]
    public void EsValido_ReceptorSinIdentificarConNumeroDistintoDeCero_DevuelveFalse(string numero)
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaB;
        comprobante.TipoDocumentoReceptor = TipoDocumento.SinIdentificar;
        comprobante.NumeroDocumentoReceptor = numero;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_CUITEmisorIncorrecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.CUITEmisor = "123";
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_PuntoVentaCero_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.PuntoVentaNumero = 0;
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_NumeroDocumentoIncorrecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.NumeroDocumentoReceptor = "123";
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_CotizacionCero_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Cotizacion = 0m;
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_ComprobanteNulo_DevuelveFalse()
    {
        var resultado = ValidadorDatosComprobante.EsValido(null);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_CUITEmisorConDigitoVerificadorIncorrecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.CUITEmisor = "20301234564";
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_CUITReceptorConDigitoVerificadorIncorrecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.NumeroDocumentoReceptor = "27301234569";
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_PESConCotizacionDistintaDeUno_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Cotizacion = 2m;
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_OtraMonedaConCotizacionPositiva_DevuelveTrue()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "DOL";
        comprobante.Cotizacion = 1000m;
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValido_CotizacionNegativa_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Cotizacion = -1m;
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_MonedaNoAdmitida_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "XXX";
        var resultado = ValidadorDatosComprobante.EsValido(comprobante);
        Assert.False(resultado);
    }


    [Fact]
    public void EsValido_FacturaExtranjeraSinCotizacionYMismaMoneda_DevuelveTrue()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "DOL";
        comprobante.Cotizacion = null;
        comprobante.CancelaEnMismaMonedaExtranjera = true;
        Assert.True(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_MonedaExtranjeraSinCotizacionYSinExcepcion_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "DOL";
        comprobante.Cotizacion = null;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_NotaCreditoSinCotizacionAunqueCancelaEnMismaMoneda_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.NotaCreditoA;
        comprobante.Moneda = "DOL";
        comprobante.Cotizacion = null;
        comprobante.CancelaEnMismaMonedaExtranjera = true;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_PESSinCotizacion_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "PES";
        comprobante.Cotizacion = null;
        comprobante.CancelaEnMismaMonedaExtranjera = true;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_NumeroComprobanteCero_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Numero = 0;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_FechaPorDefecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Fecha = default;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_TipoComprobanteInvalido_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = (TipoComprobante)999;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_ConceptoInvalido_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Concepto = (ConceptoComprobante)999;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_TipoDocumentoReceptorInvalido_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.TipoDocumentoReceptor = (TipoDocumento)999;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_MonedaNula_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = null!;
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }

    [Fact]
    public void EsValido_MonedaVacia_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Moneda = "";
        Assert.False(ValidadorDatosComprobante.EsValido(comprobante));
    }
}