using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Servicios;

namespace ArcaSim.Tests.Aplicacion.Servicios;

public class GeneradorCaeSimuladoTests
{
    private readonly GeneradorCaeSimulado _generador = new();

    [Fact]
    public void Generar_DebeGenerarCAEDe14Digitos()
    {
        var fecha = new DateOnly(2026, 10, 27);
        var resultado = _generador.Generar(fecha);
        Assert.Equal(14, resultado.CAE.Length);
        Assert.All(resultado.CAE, caracter => Assert.InRange(caracter, '0', '9'));
    }

    [Fact]
    public void Generar_DebeEstablecerVencimientoA10Dias()
    {
        var fecha = new DateOnly(2026, 10, 27);
        var resultado = _generador.Generar(fecha);
        Assert.Equal(fecha.AddDays(10), resultado.FechaVencimientoCAE);
    }

    [Fact]
    public void Generar_DebeMarcarResultadoComoAutorizado()
    {
        var fecha = new DateOnly(2026, 10, 27);
        var resultado = _generador.Generar(fecha);
        Assert.True(resultado.Autorizado);
    }

    [Fact]
    public void Generar_DebeDevolverObservacionesVacias()
    {
        var fecha = new DateOnly(2026, 10, 27);
        var resultado = _generador.Generar(fecha);
        Assert.Empty(resultado.Observaciones);
    }
}