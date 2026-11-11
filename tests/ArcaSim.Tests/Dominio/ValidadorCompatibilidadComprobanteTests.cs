using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorCompatibilidadComprobanteTests
{
    [Theory]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.ResponsableInscripto,CondicionIva.ResponsableInscripto)]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.ResponsableInscripto,CondicionIva.Monotributista)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.ResponsableInscripto,CondicionIva.ConsumidorFinal)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.ResponsableInscripto,CondicionIva.Exento)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.Monotributista,CondicionIva.ResponsableInscripto)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.Exento,CondicionIva.ConsumidorFinal)]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.ResponsableInscripto,CondicionIva.MonotributistaSocial)]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.ResponsableInscripto,
        CondicionIva.MonotributoTrabajadorIndependientePromovido)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.ResponsableInscripto,CondicionIva.IvaNoAlcanzado)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.ResponsableInscripto,CondicionIva.ClienteDelExterior)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.MonotributistaSocial,CondicionIva.ConsumidorFinal)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.Monotributista,CondicionIva.IvaLiberadoLey19640)]
    public void EsValido_CombinacionPermitida_DevuelveTrue(TipoComprobante tipo,CondicionIva emisor,
        CondicionIva receptor)
    {
        bool resultado = ValidadorCompatibilidadComprobante.EsValido(tipo, emisor, receptor);
        Assert.True(resultado);
    }

    [Theory]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.Monotributista,CondicionIva.ResponsableInscripto)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.Monotributista,CondicionIva.ConsumidorFinal)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.ResponsableInscripto,CondicionIva.ConsumidorFinal)]
    [InlineData((TipoComprobante)999,CondicionIva.ResponsableInscripto,CondicionIva.ConsumidorFinal)]
    [InlineData(TipoComprobante.FacturaB,CondicionIva.ResponsableInscripto,CondicionIva.Monotributista)]
    [InlineData(TipoComprobante.FacturaA,CondicionIva.ResponsableInscripto,CondicionIva.IvaNoAlcanzado)]
    [InlineData(TipoComprobante.FacturaC,CondicionIva.Monotributista,(CondicionIva)999)]
    public void EsValido_CombinacionNoPermitida_DevuelveFalse(TipoComprobante tipo, CondicionIva emisor,
        CondicionIva receptor)
    {
        bool resultado = ValidadorCompatibilidadComprobante.EsValido(tipo, emisor, receptor);
        Assert.False(resultado);
    }

    [Fact]
    public void EsCompatibleReceptor_FacturaAConConsumidorFinal_DevuelveFalse()
    {
        var resultado = ValidadorCompatibilidadComprobante.EsCompatibleReceptor(TipoComprobante.FacturaA,
                CondicionIva.ConsumidorFinal);
        Assert.False(resultado);
    }

    [Fact]
    public void EsCompatibleReceptor_FacturaAConResponsableInscripto_DevuelveTrue()
    {
        var resultado =
            ValidadorCompatibilidadComprobante.EsCompatibleReceptor(TipoComprobante.FacturaA,
                CondicionIva.ResponsableInscripto);
        Assert.True(resultado);
    }
}