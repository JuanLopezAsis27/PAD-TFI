using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorResultadoAutorizacionTests
{
    [Fact]
    public void EsValido_AutorizacionCorrecta_DevuelveTrue()
    {
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        Assert.True(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_CaeConLongitudIncorrecta_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_AutorizacionRechazadaSinCae_DevuelveTrue()
    {
        var resultado = new ResultadoAutorizacion
        {
            Autorizado = false
        };
        Assert.True(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_ResultadoNulo_DevuelveFalse()
    {
        Assert.False(ValidadorResultadoAutorizacion.EsValido(null));
    }

    [Fact]
    public void EsValido_CaeConLetras_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            CAE = "1234567890123A",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_AutorizacionSinCae_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_AutorizacionSinFechaVencimiento_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            Autorizado = true
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_AutorizacionRechazadaConCae_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            Autorizado = false
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void EsValido_AutorizacionRechazadaConFechaVencimiento_DevuelveFalse()
    {
        var resultado = new ResultadoAutorizacion
        {
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = false
        };
        Assert.False(ValidadorResultadoAutorizacion.EsValido(resultado));
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteYaAutorizado_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        comprobante.AplicarResultadoAutorizacion(resultado);
        Assert.Throws<InvalidOperationException>(() =>
            comprobante.AplicarResultadoAutorizacion(resultado));
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteRechazado_LanzaExcepcion()
    {
        var comprobante = new Comprobante();
        var resultado = new ResultadoAutorizacion
        {
            Autorizado = false
        };
        comprobante.AplicarResultadoAutorizacion(resultado);
        Assert.Throws<InvalidOperationException>(() =>
            comprobante.AplicarResultadoAutorizacion(resultado));
    }
}