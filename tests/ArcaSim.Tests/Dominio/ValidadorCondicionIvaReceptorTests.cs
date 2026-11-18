using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorCondicionIvaReceptorTests
{
    [Fact]
    public void Validar_CondicionReconocida_NoDevuelveError()
    {
        var error = ValidadorCondicionIvaReceptor.Validar(CondicionIva.ResponsableInscripto,
            TipoComprobante.FacturaA);
        Assert.Null(error);
    }

    [Fact]
    public void Validar_CondicionAusente_Devuelve10246()
    {
        var error = ValidadorCondicionIvaReceptor.Validar(null, TipoComprobante.FacturaA);
        Assert.NotNull(error);
        Assert.Equal("10246", error.codigo);
    }

    [Fact]
    public void Validar_CondicionFueraDelEnum_Devuelve10242()
    {
        var condicionInvalida = (CondicionIva)999;
        var error = ValidadorCondicionIvaReceptor.Validar(condicionInvalida, TipoComprobante.FacturaA);
        Assert.NotNull(error);
        Assert.Equal("10242", error.codigo);
    }
}