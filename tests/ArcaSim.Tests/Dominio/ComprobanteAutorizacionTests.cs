using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Tests.Dominio;

public class ComprobanteAutorizacionTests
{
    [Fact]
    public void AplicarResultadoAutorizacion_Aprobado_GuardaCaeYFecha()
    {
        var comprobante = new Comprobante();
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true
        };
        comprobante.AplicarResultadoAutorizacion(resultado);
        Assert.Equal("12345678901234", comprobante.CAE);
        Assert.Equal(new DateTime(2026, 11, 7), comprobante.FechaVencimientoCAE);
        Assert.Equal(EstadoAutorizacionComprobante.Autorizado, comprobante.EstadoAutorizacion);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_Rechazado_NoGuardaCae()
    {
        var comprobante = new Comprobante
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateTime(2026, 11, 7)
        };
        var resultado = new ResultadoAutorizacion
        {
            Autorizado = false
        };
        comprobante.AplicarResultadoAutorizacion(resultado);
        Assert.Null(comprobante.CAE);
        Assert.Null(comprobante.FechaVencimientoCAE);
        Assert.Equal(EstadoAutorizacionComprobante.Rechazado, comprobante.EstadoAutorizacion);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ConObservaciones_ConservaElCae()
    {
        var comprobante = new Comprobante();
        var resultado = new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 7),
            Autorizado = true,
            Observaciones = new[] { "Observación de prueba" }
        };
        comprobante.AplicarResultadoAutorizacion(resultado);
        Assert.Equal("12345678901234", comprobante.CAE);
        Assert.Single(comprobante.ObservacionesAutorizacion);
        Assert.Equal(EstadoAutorizacionComprobante.AutorizadoConObservaciones,
            comprobante.EstadoAutorizacion);
    }
}