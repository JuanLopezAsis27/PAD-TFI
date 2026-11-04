using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorAlicuotaIvaTests
{
    [Theory]
    [InlineData(AlicuotaIva.Cero, 100000, 0)]
    [InlineData(AlicuotaIva.ReducidaDosConCinco, 100000, 2500)]
    [InlineData(AlicuotaIva.ReducidaCinco, 100000, 5000)]
    [InlineData(AlicuotaIva.ReducidaDiezConCinco, 100000, 10500)]
    [InlineData(AlicuotaIva.GeneralVeintiuno, 100000, 21000)]
    [InlineData(AlicuotaIva.IncrementadaVeintisiete, 100000, 27000)]
    public void Validar_ImporteCorrecto_NoDevuelveErrores(AlicuotaIva alicuota, decimal baseImponible,
        decimal importeIVA)
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = alicuota,
            BaseImponible = baseImponible,
            ImporteIVA = importeIVA
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_ImporteIvaIncorrecto_DevuelveError10051()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 100000m,
            ImporteIVA = 18000m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Contains(errores, error => error.codigo == "10051");
    }

    [Fact]
    public void Validar_BaseImponibleCero_NoPermitida_DevuelveError10020()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 0m,
            ImporteIVA = 0m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Contains(errores, error => error.codigo == "10020");
    }

    [Fact]
    public void Validar_BaseImponibleCero_Permitida_NoDevuelveErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 0m,
            ImporteIVA = 0m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle, true);
        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_BaseImponibleNegativa_AunPermitidoCero_DevuelveError10020()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = -100m,
            ImporteIVA = 0m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle, true);
        Assert.Contains(errores, error => error.codigo == "10020");
    }

    [Fact]
    public void Validar_AlicuotaCero_ImporteCero_NoDevuelveErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.Cero,
            BaseImponible = 100000m,
            ImporteIVA = 0m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_ImporteNegativo_DevuelveError10021()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 100000m,
            ImporteIVA = -21000m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Contains(errores, error => error.codigo == "10021");
    }

    [Fact]
    public void Validar_CodigoDeAlicuotaInexistente_DevuelveError10019()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = (AlicuotaIva)999,
            BaseImponible = 100000m,
            ImporteIVA = 21000m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Contains(errores, error => error.codigo == "10019");
    }

    [Fact]
    public void Validar_DetalleNulo_DevuelveErrorDOM007()
    {
        var errores = ValidadorAlicuotaIva.Validar(null);
        Assert.Contains(errores, error => error.codigo == "DOM-007");
    }

    [Fact]
    public void Validar_DiferenciaDentroDeTolerancia_NoDevuelveErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 100m,
            ImporteIVA = 21.01m
        };

        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_DiferenciaFueraDeTolerancia_DevuelveError10051()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 100m,
            ImporteIVA = 21.02m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Contains(errores, error => error.codigo == "10051");
    }

    [Fact]
    public void Validar_BaseImponibleEImporteIvaNegativos_DevuelveAmbosErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = -100m,
            ImporteIVA = -21m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Equal(2, errores.Count);
        Assert.Contains(errores, error => error.codigo == "10020");
        Assert.Contains(errores, error => error.codigo == "10021");
    }

    [Fact]
    public void Validar_AlicuotaInvalidaEImporteIvaNegativo_DevuelveAmbosErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = (AlicuotaIva)999,
            BaseImponible = 100m,
            ImporteIVA = -21m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Equal(2, errores.Count);
        Assert.Contains(errores, error => error.codigo == "10019");
        Assert.Contains(errores, error => error.codigo == "10021");
    }

    [Fact]
    public void Validar_BaseImponibleCeroEImporteIvaNegativo_DevuelveAmbosErrores()
    {
        var detalle = new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 0m,
            ImporteIVA = -1m
        };
        var errores = ValidadorAlicuotaIva.Validar(detalle);
        Assert.Equal(2, errores.Count);
        Assert.Contains(errores, error => error.codigo == "10020");
        Assert.Contains(errores, error => error.codigo == "10021");
    }
}