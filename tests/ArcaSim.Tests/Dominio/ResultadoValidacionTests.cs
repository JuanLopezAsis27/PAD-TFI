using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ResultadoValidacionTests
{
    [Fact]
    public void Exito_SinErrores_EsValido()
    {
        var resultado = ResultadoValidacion.Exito();
        Assert.True(resultado.EsValido);
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public void ConErrores_UnError_NoEsValido()
    {
        var error = new ErrorValidacion("DOM-001", "El importe total es incorrecto.");

        var resultado = ResultadoValidacion.ConErrores(error);

        Assert.False(resultado.EsValido);
        Assert.Single(resultado.Errores);
        Assert.Equal("DOM-001", resultado.Errores[0].codigo);
    }

    [Fact]
    public void ConErrores_VariosErrores_ConservaTodos()
    {
        var resultado = ResultadoValidacion.ConErrores(new ErrorValidacion("DOM-001", "Error de importes."),
            new ErrorValidacion("DOM-002", "Error de fecha."));
        Assert.False(resultado.EsValido);
        Assert.Equal(2, resultado.Errores.Count);
    }

    [Fact]
    public void ConErrores_SinErrores_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => ResultadoValidacion.ConErrores());
    }
}