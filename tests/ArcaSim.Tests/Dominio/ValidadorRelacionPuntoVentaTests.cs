using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorRelacionPuntoVentaTests
{
    private static Contribuyente CrearContribuyente()
    {
        var contribuyente = new Contribuyente
        {
            CUIT = "20301234563",
            NombreORazonSocial = "Empresa de prueba"
        };

        var puntoVenta = new PuntoVenta
        {
            CUITContribuyente = contribuyente.CUIT,
            Numero = 1,
            Habilitado = true,
            Contribuyente = contribuyente
        };

        contribuyente.PuntosVenta.Add(puntoVenta);

        return contribuyente;
    }

    [Fact]
    public void EsValida_RelacionCorrecta_DevuelveTrue()
    {
        var contribuyente = CrearContribuyente();
        var puntoVenta = contribuyente.PuntosVenta[0];
        Assert.True(ValidadorRelacionPuntoVenta.EsValida(contribuyente, puntoVenta));
    }

    [Fact]
    public void EsValida_ContribuyenteNulo_DevuelveFalse()
    {
        var puntoVenta = new PuntoVenta();
        Assert.False(ValidadorRelacionPuntoVenta.EsValida(null, puntoVenta));
    }

    [Fact]
    public void EsValida_PuntoVentaNulo_DevuelveFalse()
    {
        var contribuyente = CrearContribuyente();
        Assert.False(ValidadorRelacionPuntoVenta.EsValida(contribuyente, null));
    }

    [Fact]
    public void EsValida_CUITDiferente_DevuelveFalse()
    {
        var contribuyente = CrearContribuyente();
        var puntoVenta = contribuyente.PuntosVenta[0];
        puntoVenta.CUITContribuyente = "27301234568";
        Assert.False(ValidadorRelacionPuntoVenta.EsValida(contribuyente, puntoVenta));
    }

    [Fact]
    public void EsValida_PuntoVentaNoRegistradoEnColeccion_DevuelveFalse()
    {
        var contribuyente = CrearContribuyente();
        var puntoVenta = new PuntoVenta
        {
            CUITContribuyente = contribuyente.CUIT,
            Numero = 2,
            Habilitado = true
        };
        Assert.False(ValidadorRelacionPuntoVenta.EsValida(contribuyente, puntoVenta));
    }

    [Fact]
    public void EsValida_ContribuyenteAsociadoConOtroCUIT_DevuelveFalse()
    {
        var contribuyente = CrearContribuyente();
        var otroContribuyente = new Contribuyente
        {
            CUIT = "27301234568"
        };

        var puntoVenta = contribuyente.PuntosVenta[0];
        puntoVenta.Contribuyente = otroContribuyente;
        Assert.False(ValidadorRelacionPuntoVenta.EsValida(contribuyente, puntoVenta));
    }
}