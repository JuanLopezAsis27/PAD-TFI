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

    [Theory]
    [InlineData("2030123456", 3)]
    [InlineData("2040111222", 8)]
    [InlineData("3071234567", 1)]
    public void CalcularDigitoVerificador_DevuelveElDigitoQueCorresponde(string primerosDiez, int esperado)
    {
        Assert.Equal(esperado, ValidadorCUIT.CalcularDigitoVerificador(primerosDiez));
    }

    [Fact]
    public void CalcularDigitoVerificador_CuandoDaDiez_DevuelveNull()
    {
        Assert.Null(ValidadorCUIT.CalcularDigitoVerificador("2030000002"));
    }

    [Theory]
    [InlineData("20301234563")]
    [InlineData("30712345671")]
    public void MotivoInvalida_CuitValida_DevuelveNull(string cuit)
    {
        Assert.Null(ValidadorCUIT.MotivoInvalida(cuit));
    }

    [Fact]
    public void MotivoInvalida_DigitoVerificadorIncorrecto_DiceCualCorresponde()
    {
        var motivo = ValidadorCUIT.MotivoInvalida("20401112229");

        Assert.NotNull(motivo);
        Assert.Contains("tiene que ser 8", motivo);
        Assert.Contains("20-40111222-8", motivo);
    }

    [Theory]
    [InlineData("20300000020", "23-30000002-9")] // hombre: 20 pasa a 23 con dígito 9
    [InlineData("27300000130", "23-30000013-4")] // mujer: 27 pasa a 23 con dígito 4
    [InlineData("30300000050", "33-30000005-9")] // persona jurídica: 30 pasa a 33 con dígito 9
    public void MotivoInvalida_DigitoDaDiez_SugiereElPrefijoQueUsaArca(string cuit, string sugerida)
    {
        var motivo = ValidadorCUIT.MotivoInvalida(cuit);

        Assert.NotNull(motivo);
        Assert.Contains(sugerida, motivo);
        Assert.True(ValidadorCUIT.EsValido(sugerida.Replace("-", "")));
    }

    [Theory]
    [InlineData("2040111222")]
    [InlineData("20-40111222-8")]
    [InlineData("2040111222A")]
    public void MotivoInvalida_NoSonOnceDigitos_LoDice(string cuit)
    {
        Assert.Contains("11 dígitos", ValidadorCUIT.MotivoInvalida(cuit));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void MotivoInvalida_Vacia_PideLaCuit(string? cuit)
    {
        Assert.Equal("Ingresá la CUIT/CUIL.", ValidadorCUIT.MotivoInvalida(cuit));
    }
}