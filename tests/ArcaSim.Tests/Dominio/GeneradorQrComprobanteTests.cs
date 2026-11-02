using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ArcaSim.Tests.Dominio;

public class GeneradorQrComprobanteTests
{
    private static DatosQrComprobante CrearDatos()
    {
        return new DatosQrComprobante
        {
            Fecha = new DateOnly(2026, 10, 28),
            CUITEmisor = "20301234563",
            PuntoVenta = 1,
            TipoComprobante = 6,
            NumeroComprobante = 42,
            ImporteTotal = 1210m,
            Moneda = "PES",
            Cotizacion = 1m,
            TipoDocumentoReceptor = (int)TipoDocumento.DNI,
            NumeroDocumentoReceptor = "12345678",
            TipoAutorizacion = "E",
            CodigoAutorizacion = "12345678901234"
        };
    }

    private static JsonDocument ObtenerJson(string url)
    {
        string base64Url = url[(url.IndexOf("?p=", StringComparison.Ordinal) + 3)..];
        string base64 = Uri.UnescapeDataString(base64Url);

        byte[] bytes = Convert.FromBase64String(base64);
        string json = Encoding.UTF8.GetString(bytes);

        return JsonDocument.Parse(json);
    }

    [Fact]
    public void GenerarUrl_DatosValidos_DevuelveUrlDeArca()
    {
        var url = GeneradorQrComprobante.GenerarUrl(CrearDatos());
        Assert.StartsWith("https://www.arca.gob.ar/fe/qr/?p=", url);
    }

    [Fact]
    public void GenerarUrl_DatosValidos_ContieneJsonCorrecto()
    {
        var url = GeneradorQrComprobante.GenerarUrl(CrearDatos());
        using var documento = ObtenerJson(url);
        var raiz = documento.RootElement;

        Assert.Equal(1, raiz.GetProperty("ver").GetInt32());
        Assert.Equal("2026-10-28", raiz.GetProperty("fecha").GetString());
        Assert.Equal(20301234563L, raiz.GetProperty("cuit").GetInt64());
        Assert.Equal(1, raiz.GetProperty("ptoVta").GetInt32());
        Assert.Equal(6, raiz.GetProperty("tipoCmp").GetInt32());
        Assert.Equal(42, raiz.GetProperty("nroCmp").GetInt64());
        Assert.Equal(1210m, raiz.GetProperty("importe").GetDecimal());
        Assert.Equal("PES", raiz.GetProperty("moneda").GetString());
        Assert.Equal(1m, raiz.GetProperty("ctz").GetDecimal());
        Assert.Equal(96, raiz.GetProperty("tipoDocRec").GetInt32());
        Assert.Equal(12345678L, raiz.GetProperty("nroDocRec").GetInt64());
        Assert.Equal("E", raiz.GetProperty("tipoCodAut").GetString());
        Assert.Equal(12345678901234L, raiz.GetProperty("codAut").GetInt64());
    }

    [Fact]
    public void GenerarUrl_SinDocumentoReceptor_OmiteCamposOpcionales()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = null,
            NumeroDocumentoReceptor = null
        };

        var url = GeneradorQrComprobante.GenerarUrl(datos);
        var base64Url = url.Split("?p=")[1];
        var base64 = Uri.UnescapeDataString(base64Url);
        var json = Encoding.UTF8.GetString(
            Convert.FromBase64String(base64));

        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        Assert.False(raiz.TryGetProperty("tipoDocRec", out _));
        Assert.False(raiz.TryGetProperty("nroDocRec", out _));
    }

    [Fact]
    public void GenerarUrl_CuitInvalido_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            CUITEmisor = "20301234564"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_CodigoAutorizacionIncorrecto_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            CodigoAutorizacion = "12345"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_PuntoVentaCero_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            PuntoVenta = 0
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_PuntoVentaMayorAlLimite_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            PuntoVenta = 100000
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_NumeroComprobanteNulo_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            NumeroComprobante = 0
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_NumeroComprobanteMayorAlLimite_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            NumeroComprobante = 100000000
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_MonedaNoAdmitida_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Moneda = "XXX"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_MonedaNula_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Moneda = null!
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_MonedaVacia_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Moneda = ""
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_CotizacionNula_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Cotizacion = 0m
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_CotizacionDecimalesExcedidos_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Cotizacion = 1000.1234567m
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_ImporteTotalNegativo_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            ImporteTotal = -1m
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_ImporteTotalDecimalesExcedidos_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            ImporteTotal = 1000.123m
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_DatosNulos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => GeneradorQrComprobante.GenerarUrl(null!));
    }

    [Fact]
    public void GenerarUrl_DocumentoDe20Digitos_GeneraQrValido()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = (int)TipoDocumento.CUIT,
            NumeroDocumentoReceptor = "12345678901234567890"
        };
        string url = GeneradorQrComprobante.GenerarUrl(datos);

        Assert.StartsWith("https://www.arca.gob.ar/fe/qr/?p=", url);

        using var documento = ObtenerJson(url);

        Assert.True(documento.RootElement.TryGetProperty("nroDocRec", out var numeroDocumento));
        Assert.Equal("12345678901234567890", numeroDocumento.GetRawText());
        Assert.Equal(JsonValueKind.Number, numeroDocumento.ValueKind);
    }

    [Fact]
    public void GenerarUrl_DocumentoDe21Digitos_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = (int)TipoDocumento.CUIT,
            NumeroDocumentoReceptor = "123456789012345678901"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_DocumentoConLetras_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = (int)TipoDocumento.CUIT,
            NumeroDocumentoReceptor = "123456789ABC"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_SoloTipoDocumentoInformado_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = (int)TipoDocumento.CUIT,
            NumeroDocumentoReceptor = null
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_FechaPredeterminada_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            Fecha = default
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_TipoComprobanteInvalido_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoComprobante = 999
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_TipoDocumentoInvalido_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = 999
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_SoloNumeroDocumentoInformado_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            TipoDocumentoReceptor = null,
            NumeroDocumentoReceptor = "12345678"
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_CodigoAutorizacionNulo_LanzaExcepcion()
    {
        var datos = CrearDatos() with
        {
            CodigoAutorizacion = null!
        };
        Assert.Throws<ArgumentException>(() => GeneradorQrComprobante.GenerarUrl(datos));
    }

    [Fact]
    public void GenerarUrl_ImporteCero_GeneraQrValido()
    {
        var datos = CrearDatos() with
        {
            ImporteTotal = 0m
        };
        string url = GeneradorQrComprobante.GenerarUrl(datos);
        using var documento = ObtenerJson(url);

        Assert.Equal(0m, documento.RootElement.GetProperty("importe").GetDecimal());
    }
}