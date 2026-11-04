using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorCUITTests
{
    [Theory]
    [InlineData("20301234563")]
    [InlineData("27301234568")]
    public void EsValido_CUITConDigitoVerificadorCorrecto_DevuelveTrue(string cuit)
    {
        Assert.True(ValidadorCUIT.EsValido(cuit));
    }

    [Theory]
    [InlineData("20301234564")]
    [InlineData("1234567890")]
    [InlineData("2030123456A")]
    [InlineData("")]
    [InlineData("   ")]
    public void EsValido_CUITInvalido_DevuelveFalse(string cuit)
    {
        Assert.False(ValidadorCUIT.EsValido(cuit));
    }

    [Fact]
    public void EsValido_CUITNulo_DevuelveFalse()
    {
        Assert.False(ValidadorCUIT.EsValido(null));
    }

    [Fact]
    public void EsValido_CUITConTodosCeros_DevuelveFalse()
    {
        Assert.False(ValidadorCUIT.EsValido("00000000000"));
    }

    [Fact]
    public void EsValido_CalculoDelDigitoDaDiez_DevuelveFalse()
    {
        Assert.False(ValidadorCUIT.EsValido("00000000400"));
    }
}