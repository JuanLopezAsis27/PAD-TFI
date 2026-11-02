using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorPuntoVentaTests
{
    private static Comprobante CrearComprobante()
    {
        return new Comprobante
        {
            CUITEmisor = "20301234563",
            PuntoVentaNumero = 1
        };
    }

    private static PuntoVenta CrearPuntoVenta()
    {
        return new PuntoVenta
        {
            CUITContribuyente = "20301234563",
            Numero = 1,
            Habilitado = true
        };
    }

    [Fact]
    public void EsValido_PuntoVentaCorrecto_DevuelveTrue()
    {
        var resultado = ValidadorPuntoVenta.EsValido(CrearPuntoVenta(), CrearComprobante());
        Assert.True(resultado);
    }

    [Fact]
    public void EsValido_PuntoVentaNulo_DevuelveFalse()
    {
        var resultado = ValidadorPuntoVenta.EsValido(null, CrearComprobante());
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_ComprobanteNulo_DevuelveFalse()
    {
        var resultado = ValidadorPuntoVenta.EsValido(CrearPuntoVenta(), null);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_PuntoVentaDeshabilitado_DevuelveFalse()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.Habilitado = false;

        var resultado = ValidadorPuntoVenta.EsValido(puntoVenta, CrearComprobante());
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_NumeroDiferente_DevuelveFalse()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.Numero = 2;

        var resultado = ValidadorPuntoVenta.EsValido(puntoVenta, CrearComprobante());
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_CUITDiferente_DevuelveFalse()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.CUITContribuyente = "27301234568";

        var resultado = ValidadorPuntoVenta.EsValido(puntoVenta, CrearComprobante());
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_NumeroCero_DevuelveFalse()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.Numero = 0;

        var resultado = ValidadorPuntoVenta.EsValido(puntoVenta, CrearComprobante());
        Assert.False(resultado);
    }
}