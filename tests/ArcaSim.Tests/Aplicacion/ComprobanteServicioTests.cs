using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

public class ComprobanteServicioTests
{
    private readonly RepositorioContribuyenteFalso _contribuyenteRepositorio;
    private readonly RepositorioPuntoVentaFalso _puntoVentaRepositorio;
    private readonly RepositorioComprobanteFalso _comprobanteRepositorio;
    private readonly ComprobanteServicio _servicio;

    private readonly Contribuyente _contribuyente;
    private readonly PuntoVenta _puntoVenta;

    private static readonly DateTime FechaPrueba = new(2026, 10, 27);

    public ComprobanteServicioTests()
    {
        _contribuyenteRepositorio = new RepositorioContribuyenteFalso();
        _puntoVentaRepositorio = new RepositorioPuntoVentaFalso();
        _comprobanteRepositorio = new RepositorioComprobanteFalso();

        _servicio = new ComprobanteServicio(_contribuyenteRepositorio, _puntoVentaRepositorio,
            _comprobanteRepositorio);

        _contribuyente = CrearContribuyente();
        _puntoVenta = CrearPuntoVenta();

        _contribuyente.PuntosVenta.Add(_puntoVenta);
        _contribuyenteRepositorio.Agregar(_contribuyente);
        _puntoVentaRepositorio.Agregar(_puntoVenta);
    }

    [Fact]
    public void Registrar_ComprobanteValido_RegistraCorrectamente()
    {
        var comprobante = CrearComprobante();

        var resultado = _servicio.Registrar(comprobante, FechaPrueba);

        Assert.True(resultado.EsValido);
        Assert.Contains(comprobante, _comprobanteRepositorio.Comprobantes);
        Assert.Contains(comprobante, _puntoVenta.Comprobantes);
        Assert.Same(_puntoVenta, comprobante.PuntoVenta);
    }

    [Fact]
    public void Registrar_ComprobanteNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => _servicio.Registrar(null!, FechaPrueba));
    }

    [Fact]
    public void Registrar_ContribuyenteInexistente_DevuelveError()
    {
        _contribuyenteRepositorio.Contribuyentes.Clear();
        var resultado = _servicio.Registrar(CrearComprobante(), FechaPrueba);
        Assert.False(resultado.EsValido);
        Assert.Empty(_comprobanteRepositorio.Comprobantes);
    }

    [Fact]
    public void Registrar_PuntoVentaInexistente_DevuelveError()
    {
        _puntoVentaRepositorio.PuntosVenta.Clear();
        var resultado = _servicio.Registrar(CrearComprobante(), FechaPrueba);
        Assert.False(resultado.EsValido);
        Assert.Empty(_comprobanteRepositorio.Comprobantes);
    }

    [Fact]
    public void Registrar_ComprobanteDuplicado_DevuelveError()
    {
        var existente = CrearComprobante();
        _comprobanteRepositorio.Agregar(existente);
        var duplicado = CrearComprobante();
        var resultado = _servicio.Registrar(duplicado, FechaPrueba);
        Assert.False(resultado.EsValido);
        Assert.Single(_comprobanteRepositorio.Comprobantes);
    }

    [Fact]
    public void Registrar_ComprobanteInvalido_NoLoRegistra()
    {
        var comprobante = CrearComprobante();
        comprobante.ImporteTotal = 50000;
        var resultado = _servicio.Registrar(comprobante, FechaPrueba);
        Assert.False(resultado.EsValido);
        Assert.Empty(_comprobanteRepositorio.Comprobantes);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteAutorizado_ActualizaCorrectamente()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultadoAutorizacion = new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        };
        _servicio.AplicarResultadoAutorizacion(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero, resultadoAutorizacion);
        Assert.Equal(EstadoAutorizacionComprobante.Autorizado, comprobante.EstadoAutorizacion);
        Assert.Equal("12345678901234", comprobante.CAE);
        Assert.Equal(new DateTime(2026, 11, 10), comprobante.FechaVencimientoCAE);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ConObservaciones_ActualizaEstado()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultadoAutorizacion = new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10),
            Observaciones = new[] { "Observación de prueba." }
        };
        _servicio.AplicarResultadoAutorizacion(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero, resultadoAutorizacion);
        Assert.Equal(EstadoAutorizacionComprobante.AutorizadoConObservaciones,
            comprobante.EstadoAutorizacion);
        Assert.Contains("Observación de prueba.", comprobante.ObservacionesAutorizacion);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteRechazado_ActualizaEstado()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultadoAutorizacion = new ResultadoAutorizacion
        {
            Autorizado = false,
            Observaciones = new[] { "Comprobante rechazado." }
        };
        _servicio.AplicarResultadoAutorizacion(
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero,
            resultadoAutorizacion);
        Assert.Equal(EstadoAutorizacionComprobante.Rechazado, comprobante.EstadoAutorizacion);
        Assert.Null(comprobante.CAE);
        Assert.Null(comprobante.FechaVencimientoCAE);
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteInexistente_LanzaExcepcion()
    {
        var resultadoAutorizacion = new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        };
        Assert.Throws<InvalidOperationException>(() => _servicio.AplicarResultadoAutorizacion(
            "20301234563", 1, TipoComprobante.FacturaA, 1, resultadoAutorizacion));
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ResultadoNulo_LanzaExcepcion()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        Assert.Throws<ArgumentNullException>(() => _servicio.AplicarResultadoAutorizacion(
                comprobante.CUITEmisor, comprobante.PuntoVentaNumero, comprobante.Tipo,
                comprobante.Numero, null!));
    }

    [Fact]
    public void AplicarResultadoAutorizacion_ComprobanteYaProcesado_LanzaExcepcion()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);

        var primerResultado = new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        };

        var segundoResultado = new ResultadoAutorizacion
        {
            Autorizado = false
        };

        _servicio.AplicarResultadoAutorizacion(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero, primerResultado);

        Assert.Throws<InvalidOperationException>(() => _servicio.AplicarResultadoAutorizacion(
                comprobante.CUITEmisor, comprobante.PuntoVentaNumero, comprobante.Tipo,
                comprobante.Numero, segundoResultado));
    }

    [Fact]
    public void Registrar_NumeroSiguienteAlUltimoAutorizado_RegistraCorrectamente()
    {
        var anterior = CrearComprobante();
        anterior.Numero = 1;
        anterior.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(anterior);
        var nuevo = CrearComprobante();
        nuevo.Numero = 2;
        var resultado = _servicio.Registrar(nuevo, FechaPrueba);
        Assert.True(resultado.EsValido);
        Assert.Contains(nuevo, _comprobanteRepositorio.Comprobantes);
    }

    [Fact]
    public void Consultar_ComprobanteExistente_DevuelveComprobante()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Consultar(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero);
        Assert.Same(comprobante, resultado);
    }

    [Fact]
    public void Consultar_ComprobanteInexistente_DevuelveNull()
    {
        var resultado = _servicio.Consultar("20301234563", 1, TipoComprobante.FacturaB, 99);
        Assert.Null(resultado);
    }

    [Fact]
    public void ConsultarUltimoNumeroAutorizado_SinComprobantes_DevuelveCero()
    {
        var resultado = _servicio.ConsultarUltimoNumeroAutorizado(
            "20301234563", 1, TipoComprobante.FacturaB);
        Assert.Equal(0, resultado);
    }

    [Fact]
    public void ConsultarUltimoNumeroAutorizado_ConComprobantes_DevuelveElUltimoAutorizado()
    {
        var anterior = CrearComprobante();
        anterior.Numero = 1;
        anterior.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        var siguiente = CrearComprobante();
        siguiente.Numero = 2;
        _comprobanteRepositorio.Agregar(anterior);
        _comprobanteRepositorio.Agregar(siguiente);
        var resultado = _servicio.ConsultarUltimoNumeroAutorizado(anterior.CUITEmisor,
            anterior.PuntoVentaNumero, anterior.Tipo);
        Assert.Equal(1, resultado);
    }

    [Fact]
    public void Constatar_ComprobanteAutorizadoYCAECorrecto_RetornaValido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "12345678901234");
        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void Constatar_ComprobanteInexistente_RetornaInvalido()
    {
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "12345678901234");
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no existe.", resultado.Mensaje);
    }

    [Fact]
    public void Constatar_ComprobantePendiente_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "12345678901234");
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no se encuentra autorizado.", resultado.Mensaje);
    }

    [Fact]
    public void Constatar_CAEIncorrecto_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "99999999999999");
        Assert.False(resultado.EsValido);
        Assert.Equal("El CAE informado no coincide con el registrado.", resultado.Mensaje);
    }

    [Fact]
    public void Constatar_ComprobanteAutorizadoConObservacionesYCAECorrecto_RetornaValido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10),
            Observaciones = ["Observación de prueba."]
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "12345678901234");
        Assert.True(resultado.EsValido);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constatar_CAEVacioONulo_RetornaInvalido(string? cae)
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1, cae!);
        Assert.False(resultado.EsValido);
        Assert.Equal("El CAE informado no coincide con el registrado.", resultado.Mensaje);
    }

    [Fact]
    public void Constatar_ComprobanteRechazado_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = false,
            Observaciones = ["Comprobante rechazado."]
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.Constatar("20301234563", 1, TipoComprobante.FacturaA, 1,
            "12345678901234");
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no se encuentra autorizado.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_ComprobanteAutorizado_RetornaValido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var urlQr = CrearUrlQr();
        var resultado = _servicio.ConstatarPorQr(urlQr);
        Assert.True(resultado.EsValido);
        Assert.Equal("El comprobante es válido.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_ComprobanteConObservaciones_RetornaValido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10),
            Observaciones = ["Observación de prueba."]
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQr());
        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void ConstatarPorQr_ComprobanteInexistente_RetornaInvalido()
    {
        var resultado = _servicio.ConstatarPorQr(CrearUrlQr());
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no existe.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_ComprobantePendiente_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQr());
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no se encuentra autorizado.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_CAEDiferente_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQr(cae: "99999999999999"));
        Assert.False(resultado.EsValido);
        Assert.Equal("El CAE informado no coincide con el registrado.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_ComprobanteRechazado_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = false,
            Observaciones = ["Comprobante rechazado."]
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQr());
        Assert.False(resultado.EsValido);
        Assert.Equal("El comprobante no se encuentra autorizado.", resultado.Mensaje);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-es-una-url")]
    [InlineData("https://www.google.com/")]
    [InlineData("https://www.arca.gob.ar/otra-ruta/")]
    public void ConstatarPorQr_URLInvalida_RetornaInvalido(string url)
    {
        var resultado = _servicio.ConstatarPorQr(url);
        Assert.False(resultado.EsValido);
        Assert.Equal("La URL del código QR no es válida.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_SinParametroP_RetornaInvalido()
    {
        var resultado = _servicio.ConstatarPorQr("https://www.arca.gob.ar/fe/qr/");
        Assert.False(resultado.EsValido);
        Assert.Equal("La URL del código QR no contiene los datos necesarios.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_Base64Invalido_RetornaInvalido()
    {
        var resultado = _servicio.ConstatarPorQr("https://www.arca.gob.ar/fe/qr/?p=!!!");
        Assert.False(resultado.EsValido);
        Assert.Equal("No se pudieron interpretar los datos del código QR.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_JSONInvalido_RetornaInvalido()
    {
        var url = CrearUrlQrConJson("esto no es un json");
        var resultado = _servicio.ConstatarPorQr(url);
        Assert.False(resultado.EsValido);
        Assert.Equal("No se pudieron interpretar los datos del código QR.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_FaltaCampoObligatorio_RetornaInvalido()
    {
        var json = """
            {
            "cuit": "20301234563",
            "ptoVta": 1,
            "tipoCmp": 1,
            "nroCmp": 1
            }
            """;
        var resultado = _servicio.ConstatarPorQr(CrearUrlQrConJson(json));
        Assert.False(resultado.EsValido);
        Assert.Equal("No se pudieron interpretar los datos del código QR.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_CampoConFormatoIncorrecto_RetornaInvalido()
    {
        var json = """
            {
            "cuit": "20301234563",
            "ptoVta": "incorrecto",
            "tipoCmp": 1,
            "nroCmp": 1,
            "codAut": "12345678901234"
            }
            """;
        var resultado = _servicio.ConstatarPorQr(CrearUrlQrConJson(json));
        Assert.False(resultado.EsValido);
        Assert.Equal("No se pudieron interpretar los datos del código QR.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_QRGeneradoPorElSistema_RetornaValido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);

        var datosQr = new DatosQrComprobante
        {
            Fecha = DateOnly.FromDateTime(comprobante.Fecha),
            CUITEmisor = comprobante.CUITEmisor,
            PuntoVenta = comprobante.PuntoVentaNumero,
            TipoComprobante = (int)comprobante.Tipo,
            NumeroComprobante = comprobante.Numero,
            ImporteTotal = comprobante.ImporteTotal,
            CodigoAutorizacion = comprobante.CAE!,
            TipoDocumentoReceptor = (int)comprobante.TipoDocumentoReceptor,
            NumeroDocumentoReceptor = comprobante.NumeroDocumentoReceptor,
            Moneda = comprobante.Moneda,
            Cotizacion = comprobante.Cotizacion!.Value,
            TipoAutorizacion = "E",
        };
        var url = GeneradorQrComprobante.GenerarUrl(datosQr);
        var resultado = _servicio.ConstatarPorQr(url);
        Assert.True(resultado.EsValido);
        Assert.Equal("El comprobante es válido.", resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_QRGeneradoConCAEDiferente_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var datosQr = new DatosQrComprobante
        {
            Fecha = DateOnly.FromDateTime(comprobante.Fecha),
            CUITEmisor = comprobante.CUITEmisor,
            PuntoVenta = comprobante.PuntoVentaNumero,
            TipoComprobante = (int)comprobante.Tipo,
            NumeroComprobante = comprobante.Numero,
            ImporteTotal = comprobante.ImporteTotal,
            CodigoAutorizacion = "99999999999999"
        };
        var url = GeneradorQrComprobante.GenerarUrl(datosQr);
        var resultado = _servicio.ConstatarPorQr(url);
        Assert.False(resultado.EsValido);
        Assert.Equal("El CAE informado no coincide con el registrado.", resultado.Mensaje);
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteExistente_DebeAutorizarlo()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        _servicio.AutorizarSimuladamente(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero);
        Assert.Equal(EstadoAutorizacionComprobante.Autorizado, comprobante.EstadoAutorizacion);
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteExistente_DebeAsignarCAEDe14Digitos()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        _servicio.AutorizarSimuladamente(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero);
        Assert.NotNull(comprobante.CAE);
        Assert.Equal(14, comprobante.CAE.Length);
        Assert.All(comprobante.CAE, caracter => Assert.InRange(caracter, '0', '9'));
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteExistente_DebeAsignarVencimientoCorrecto()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        _servicio.AutorizarSimuladamente(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero);
        var fechaEsperada = comprobante.Fecha.Date.AddDays(10);
        Assert.Equal(fechaEsperada, comprobante.FechaVencimientoCAE);
        Assert.Equal(fechaEsperada, comprobante.FechaVencimientoCAE);
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteExistente_DebeActualizarRepositorio()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        _servicio.AutorizarSimuladamente(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
            comprobante.Tipo, comprobante.Numero);
        var comprobanteGuardado = _comprobanteRepositorio.ObtenerPorIdentificacion(comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero, comprobante.Tipo, comprobante.Numero);
        Assert.NotNull(comprobanteGuardado);
        Assert.Equal(EstadoAutorizacionComprobante.Autorizado, comprobanteGuardado.EstadoAutorizacion);
        Assert.Equal(comprobante.CAE, comprobanteGuardado.CAE);
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteInexistente_DebeLanzarExcepcion()
    {
        var excepcion = Assert.Throws<InvalidOperationException>(() =>
            _servicio.AutorizarSimuladamente(_contribuyente.CUIT, _puntoVenta.Numero,
            TipoComprobante.FacturaA, 999));
        Assert.Equal("No existe el comprobante indicado.", excepcion.Message);
    }

    [Fact]
    public void AutorizarSimuladamente_ComprobanteYaAutorizado_DebeLanzarExcepcion()
    {
        var comprobante = CrearComprobante();
        _comprobanteRepositorio.Agregar(comprobante);
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            CAE = "12345678901234",
            FechaVencimientoCAE = DateOnly.FromDateTime(comprobante.Fecha).AddDays(10),
            Autorizado = true
        });
        var excepcion = Assert.Throws<InvalidOperationException>(() =>
            _servicio.AutorizarSimuladamente(
                comprobante.CUITEmisor,
                comprobante.PuntoVentaNumero,
                comprobante.Tipo,
                comprobante.Numero));
        Assert.Equal("El comprobante ya se encuentra autorizado.", excepcion.Message);
        Assert.Equal("12345678901234", comprobante.CAE);
    }

    [Fact]
    public void ConstatarPorQr_ImporteAlterado_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });

        _comprobanteRepositorio.Agregar(comprobante);

        var datos = new
        {
            ver = 1,
            fecha = FechaPrueba.ToString("yyyy-MM-dd"),
            cuit = 20301234563L,
            ptoVta = 1,
            tipoCmp = (int)TipoComprobante.FacturaA,
            nroCmp = 1,
            importe = 999m,
            moneda = "PES",
            ctz = 1m,
            tipoDocRec = (int)TipoDocumento.CUIT,
            nroDocRec = 20301234563L,
            tipoCodAut = "E",
            codAut = 12345678901234L
        };

        var json = JsonSerializer.Serialize(datos);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQrConJson(json));

        Assert.False(resultado.EsValido);
        Assert.Equal(
            "Los datos del código QR no coinciden con el comprobante registrado.",
            resultado.Mensaje);
    }

    [Fact]
    public void ConstatarPorQr_FechaAlterada_RetornaInvalido()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = "12345678901234",
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobanteRepositorio.Agregar(comprobante);
        var datos = new
        {
            ver = 1,
            fecha = "2026-10-28",
            cuit = 20301234563L,
            ptoVta = 1,
            tipoCmp = (int)TipoComprobante.FacturaA,
            nroCmp = 1,
            importe = 121000m,
            moneda = "PES",
            ctz = 1m,
            tipoDocRec = (int)TipoDocumento.CUIT,
            nroDocRec = 20301234563L,
            tipoCodAut = "E",
            codAut = 12345678901234L
        };
        var json = JsonSerializer.Serialize(datos);
        var resultado = _servicio.ConstatarPorQr(CrearUrlQrConJson(json));
        Assert.False(resultado.EsValido);
        Assert.Equal("Los datos del código QR no coinciden con el comprobante registrado.",
            resultado.Mensaje);
    }

    private static string CrearUrlQr(string cae = "12345678901234")
    {
        var datos = new
        {
            ver = 1,
            fecha = FechaPrueba.ToString("yyyy-MM-dd"),
            cuit = 20301234563L,
            ptoVta = 1,
            tipoCmp = (int)TipoComprobante.FacturaA,
            nroCmp = 1,
            importe = 121000m,
            moneda = "PES",
            ctz = 1m,
            tipoDocRec = (int)TipoDocumento.CUIT,
            nroDocRec = 20301234563L,
            tipoCodAut = "E",
            codAut = long.Parse(cae, CultureInfo.InvariantCulture)
        };
        var json = JsonSerializer.Serialize(datos);
        return CrearUrlQrConJson(json);
    }

    private static string CrearUrlQrConJson(string json)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        return $"https://www.arca.gob.ar/fe/qr/?p={Uri.EscapeDataString(base64)}";
    }

    private static Contribuyente CrearContribuyente()
    {
        return new Contribuyente
        {
            CUIT = "20301234563",
            NombreORazonSocial = "Empresa de prueba",
            TipoPersona = Enum.GetValues<TipoPersona>()[0],
            CondicionIva = CondicionIva.ResponsableInscripto
        };
    }

    private PuntoVenta CrearPuntoVenta()
    {
        return new PuntoVenta
        {
            CUITContribuyente = _contribuyente.CUIT,
            Numero = 1,
            Habilitado = true,
            Contribuyente = _contribuyente
        };
    }

    private Comprobante CrearComprobante()
    {
        return new Comprobante
        {
            CUITEmisor = _contribuyente.CUIT,
            Concepto = ConceptoComprobante.Productos,
            PuntoVentaNumero = _puntoVenta.Numero,
            Tipo = TipoComprobante.FacturaA,
            Numero = 1,
            Fecha = FechaPrueba,
            TipoDocumentoReceptor = TipoDocumento.CUIT,
            NumeroDocumentoReceptor = _contribuyente.CUIT,
            CondicionIvaReceptor = CondicionIva.ResponsableInscripto,
            ImporteNeto = 100000,
            ImporteNoGravado = 0,
            ImporteExento = 0,
            ImporteIVA = 21000,
            ImporteTributos = 0,
            ImporteTotal = 121000,
            Moneda = "PES",
            Cotizacion = 1,
            AlicuotasIva = new List<ComprobanteIva>
            {
                new()
                {
                    Alicuota = AlicuotaIva.GeneralVeintiuno,
                    BaseImponible = 100000,
                    ImporteIVA = 21000
                }
            }
        };
    }
}

internal class RepositorioComprobanteFalso : IComprobanteRepositorio
{
    public List<Comprobante> Comprobantes { get; } = new();

    public long UltimoNumeroAutorizado { get; set; }

    public Comprobante? ObtenerPorIdentificacion(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo, long numero)
    {
        return Comprobantes.FirstOrDefault(c => c.CUITEmisor == cuitEmisor &&
            c.PuntoVentaNumero == puntoVentaNumero && c.Tipo == tipo && c.Numero == numero);
    }

    public long ObtenerUltimoNumeroAutorizado(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo)
    {
        return Comprobantes
            .Where(c => c.CUITEmisor == cuitEmisor && c.PuntoVentaNumero == puntoVentaNumero &&
                c.Tipo == tipo &&
                c.EstadoAutorizacion is
                EstadoAutorizacionComprobante.Autorizado or
                EstadoAutorizacionComprobante.AutorizadoConObservaciones)
            .Select(c => c.Numero)
            .DefaultIfEmpty(0)
            .Max();
    }

    public void Agregar(Comprobante comprobante)
    {
        Comprobantes.Add(comprobante);
    }

    public void Actualizar(Comprobante comprobante)
    {
        int indice = Comprobantes.FindIndex(c => c.CUITEmisor == comprobante.CUITEmisor &&
            c.PuntoVentaNumero == comprobante.PuntoVentaNumero && c.Tipo == comprobante.Tipo &&
            c.Numero == comprobante.Numero);

        if (indice >= 0)
            Comprobantes[indice] = comprobante;
    }
}