using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorImportesComprobanteTests
{
    private static Comprobante CrearComprobanteValido()
    {
        return new Comprobante
        {
            ImporteNeto = 100000m,
            ImporteNoGravado = 0m,
            ImporteExento = 0m,
            ImporteIVA = 21000m,
            ImporteTributos = 0m,
            ImporteTotal = 121000m
        };
    }

    [Fact]
    public void Validar_ImportesNoNegativos_NoDevuelveErrores()
    {
        var comprobante = CrearComprobanteValido();
        var errores = ValidadorImportesComprobante.Validar(comprobante);
        Assert.Empty(errores);
    }

    [Theory]
    [InlineData("neto", "10045")]
    [InlineData("noGravado", "10043")]
    [InlineData("exento", "10044")]
    [InlineData("tributos", "10046")]
    [InlineData("iva", "10047")]
    public void Validar_ImporteNegativo_DevuelveErrorCorrespondiente(string campo, string codigoEsperado)
    {
        var comprobante = CrearComprobanteValido();

        switch (campo)
        {
            case "neto":
                comprobante.ImporteNeto = -1m;
                break;
            case "noGravado":
                comprobante.ImporteNoGravado = -1m;
                break;
            case "exento":
                comprobante.ImporteExento = -1m;
                break;
            case "tributos":
                comprobante.ImporteTributos = -1m;
                break;
            case "iva":
                comprobante.ImporteIVA = -1m;
                break;
        }
        var errores = ValidadorImportesComprobante.Validar(comprobante);
        Assert.Contains(errores, error => error.codigo == codigoEsperado);
    }
}