using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorFechaComprobanteTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(5)]
    public void EsValida_ProductosDentroDelMargen_DevuelveTrue(int diferenciaDias)
    {
        var generacion = new DateTime(2026, 10, 10);
        var fecha = generacion.AddDays(diferenciaDias);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Productos);
        Assert.True(resultado);
    }

    [Theory]
    [InlineData(-6)]
    [InlineData(6)]
    public void EsValida_ProductosFueraDelMargen_DevuelveFalse(int diferenciaDias)
    {
        var generacion = new DateTime(2026, 10, 10);
        var fecha = generacion.AddDays(diferenciaDias);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Productos);
        Assert.False(resultado);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(10)]
    public void EsValida_ServiciosDentroDelMargenConFechas_DevuelveTrue(int diferenciaDias)
    {
        var generacion = new DateTime(2026, 10, 10);
        var fecha = generacion.AddDays(diferenciaDias);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Servicios, generacion.AddDays(-2), generacion.AddDays(-1), fecha);
        Assert.True(resultado);
    }

    [Theory]
    [InlineData(-11)]
    [InlineData(11)]
    public void EsValida_ServiciosFueraDelMargen_DevuelveFalse(int diferenciaDias)
    {
        var generacion = new DateTime(2026, 10, 10);
        var fecha = generacion.AddDays(diferenciaDias);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Servicios, generacion.AddDays(-2), generacion.AddDays(-1), generacion);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_ServiciosSinFechas_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.Servicios);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_ProductosYServiciosConFechas_DevuelveTrue()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.ProductosYServicios, generacion.AddDays(-2), generacion.AddDays(-1),
            generacion);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValida_ProductosSinFechasDeServicio_DevuelveTrue()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.Productos);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValida_ProductosConFechasDeServicio_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.Productos, generacion.AddDays(-2), generacion.AddDays(-1), generacion);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_FechaInicioPosteriorAFinServicio_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.Servicios, generacion.AddDays(2), generacion.AddDays(1),
            generacion.AddDays(3));
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_FechaPorDefecto_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 10);
        bool resultado = ValidadorFechaComprobante.EsValida(default, generacion,
            ConceptoComprobante.Productos);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_ProductoEnMesSiguiente_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 30);
        var fecha = new DateTime(2026, 11, 1);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Productos);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValida_ServicioEnMesSiguiente_DevuelveTrue()
    {
        var generacion = new DateTime(2026, 10, 30);
        var fecha = new DateTime(2026, 11, 2);
        bool resultado = ValidadorFechaComprobante.EsValida(fecha, generacion,
            ConceptoComprobante.Servicios, generacion.AddDays(-2), generacion.AddDays(-1), fecha);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValida_FechaGeneracionPorDefecto_DevuelveFalse()
    {
        var fecha = new DateTime(2026, 10, 10);
        Assert.False(ValidadorFechaComprobante.EsValida(fecha, default, ConceptoComprobante.Productos));
    }

    [Fact]
    public void EsValida_VencimientoAnteriorAFechaComprobante_DevuelveFalse()
    {
        var generacion = new DateTime(2026, 10, 10);
        Assert.False(ValidadorFechaComprobante.EsValida(generacion, generacion,
            ConceptoComprobante.Servicios, generacion.AddDays(-2), generacion.AddDays(-1),
            generacion.AddDays(-1)));
    }

    [Fact]
    public void EsValida_VencimientoIgualAFechaComprobante_DevuelveTrue()
    {
        var fecha = new DateTime(2026, 10, 10);
        Assert.True(ValidadorFechaComprobante.EsValida(fecha, fecha, ConceptoComprobante.Servicios,
            fecha.AddDays(-2), fecha.AddDays(-1), fecha));
    }
}