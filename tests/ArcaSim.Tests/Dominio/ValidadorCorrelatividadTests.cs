using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorCorrelatividadTests
{
    [Theory]
    [InlineData(15, 16)]
    [InlineData(0, 1)]
    [InlineData(99, 100)]
    public void EsSiguienteNumero_NumeroCorrelativo_DevuelveTrue(long ultimo, long nuevo)
    {
        bool resultado = ValidadorCorrelatividad.EsSiguienteNumero(ultimo, nuevo);
        Assert.True(resultado);
    }

    [Theory]
    [InlineData(15, 15)]
    [InlineData(15, 17)]
    [InlineData(15, 14)]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    public void EsSiguienteNumero_NumeroIncorrecto_DevuelveFalse(long ultimo, long nuevo)
    {
        bool resultado = ValidadorCorrelatividad.EsSiguienteNumero(ultimo, nuevo);
        Assert.False(resultado);
    }

    [Fact]
    public void EsSiguienteNumero_UltimoNumeroMaximo_DevuelveFalse()
    {
        bool resultado = ValidadorCorrelatividad.EsSiguienteNumero(long.MaxValue, long.MaxValue);
        Assert.False(resultado);
    }
}