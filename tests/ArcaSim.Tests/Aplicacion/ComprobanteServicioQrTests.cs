using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Aplicacion;

/// <summary>
/// El QR de los comprobantes lleva a la constatación del portal (no a ARCA real), y la constatación acepta
/// tanto esas URL como las de ARCA. También el listado de comprobantes por emisor.
/// </summary>
public class ComprobanteServicioQrTests
{
    private const string Portal = "https://arcasim.example.com";
    private const string Cuit = "20301234563";
    private const string Cae = "12345678901234";

    private readonly RepositorioComprobanteFalso _comprobantes = new();
    private readonly ComprobanteServicio _servicio;
    private readonly ComprobanteServicio _servicioSinPortal;

    public ComprobanteServicioQrTests()
    {
        var contribuyentes = new RepositorioContribuyenteFalso();
        var puntosVenta = new RepositorioPuntoVentaFalso();

        _servicio = new ComprobanteServicio(contribuyentes, puntosVenta, _comprobantes,
            OpcionesQr.ParaPortal(Portal));
        _servicioSinPortal = new ComprobanteServicio(contribuyentes, puntosVenta, _comprobantes);
    }

    [Fact]
    public void GenerarUrlQr_ComprobanteAutorizado_LlevaALaConstatacionDelPortal()
    {
        var url = _servicio.GenerarUrlQr(AgregarAutorizado());

        Assert.NotNull(url);
        Assert.StartsWith("https://arcasim.example.com/fe/qr/?p=", url);
    }

    [Fact]
    public void GenerarUrlQr_LosDatosSonLosMismosQueConLaUrlDeArca()
    {
        var comprobante = AgregarAutorizado();

        var urlPortal = _servicio.GenerarUrlQr(comprobante)!;
        var urlArca = _servicioSinPortal.GenerarUrlQr(comprobante)!;

        Assert.StartsWith(GeneradorQrComprobante.UrlBaseArca + "?p=", urlArca);
        Assert.Equal(ParametroP(urlArca), ParametroP(urlPortal));
    }

    [Fact]
    public void GenerarUrlQr_ComprobantePendiente_DevuelveNull()
    {
        Assert.Null(_servicio.GenerarUrlQr(CrearComprobante()));
    }

    [Fact]
    public void GenerarUrlQr_AutorizadoSinCotizacion_DevuelveNullSinLanzar()
    {
        // Moneda extranjera que se cancela en esa moneda: la cotización puede venir vacía.
        var comprobante = AgregarAutorizado();
        comprobante.Cotizacion = null;

        Assert.Null(_servicio.GenerarUrlQr(comprobante));
    }

    [Fact]
    public void ConstatarPorQr_UrlGeneradaPorElPortal_EsValidaYDevuelveElComprobante()
    {
        var comprobante = AgregarAutorizado();

        var resultado = _servicio.ConstatarPorQr(_servicio.GenerarUrlQr(comprobante)!);

        Assert.True(resultado.EsValido, resultado.Mensaje);
        Assert.Same(comprobante, resultado.Comprobante);
    }

    [Fact]
    public void ConstatarPorQr_PortalDetrasDeUnProxyPorHttp_EsValida()
    {
        var url = _servicio.GenerarUrlQr(AgregarAutorizado())!.Replace("https://", "http://");

        Assert.True(_servicio.ConstatarPorQr(url).EsValido);
    }

    [Fact]
    public void ConstatarPorQr_UrlDeArcaConPortalConfigurado_EsValida()
    {
        // Un sistema de otro grupo que sigue la especificación al pie de la letra imprime la URL de ARCA.
        var p = ParametroP(_servicio.GenerarUrlQr(AgregarAutorizado())!);

        var resultado = _servicio.ConstatarPorQr($"{GeneradorQrComprobante.UrlBaseArca}?p={p}");

        Assert.True(resultado.EsValido, resultado.Mensaje);
    }

    [Theory]
    [InlineData("https://otro-sitio.com/fe/qr/")]
    [InlineData("https://arcasim.example.com/otra-ruta/")]
    [InlineData("ftp://arcasim.example.com/fe/qr/")]
    public void ConstatarPorQr_OtraPaginaConLosMismosDatos_EsInvalida(string urlBase)
    {
        var p = ParametroP(_servicio.GenerarUrlQr(AgregarAutorizado())!);

        var resultado = _servicio.ConstatarPorQr($"{urlBase}?p={p}");

        Assert.False(resultado.EsValido);
        Assert.Equal("La URL del código QR no es válida.", resultado.Mensaje);
        Assert.Null(resultado.Comprobante);
    }

    [Fact]
    public void ConstatarPorQr_UrlDelPortalSinPortalConfigurado_EsInvalida()
    {
        var url = _servicio.GenerarUrlQr(AgregarAutorizado())!;

        Assert.False(_servicioSinPortal.ConstatarPorQr(url).EsValido);
    }

    [Fact]
    public void ConstatarPorQr_SinPortalConfigurado_ArcaPorHttpEsInvalida()
    {
        var url = _servicioSinPortal.GenerarUrlQr(AgregarAutorizado())!.Replace("https://", "http://");

        Assert.False(_servicioSinPortal.ConstatarPorQr(url).EsValido);
    }

    [Fact]
    public void Constatar_CaeCorrecto_DevuelveElComprobante()
    {
        var comprobante = AgregarAutorizado();

        var resultado = _servicio.Constatar(Cuit, 1, TipoComprobante.FacturaA, 1, Cae);

        Assert.True(resultado.EsValido);
        Assert.Same(comprobante, resultado.Comprobante);
    }

    [Fact]
    public void Constatar_CaeIncorrecto_NoDevuelveElComprobante()
    {
        AgregarAutorizado();

        var resultado = _servicio.Constatar(Cuit, 1, TipoComprobante.FacturaA, 1, "99999999999999");

        Assert.False(resultado.EsValido);
        Assert.Null(resultado.Comprobante);
    }

    [Fact]
    public void ListarPorEmisores_DevuelveSoloLosDeEsosEmisoresDelMasRecienteAlMasViejo()
    {
        var viejo = CrearComprobante(numero: 1, fecha: new DateTime(2026, 11, 1));
        var nuevo = CrearComprobante(numero: 2, fecha: new DateTime(2026, 11, 2));
        var deOtro = CrearComprobante(numero: 1, fecha: new DateTime(2026, 11, 3));
        deOtro.CUITEmisor = "30712345671";
        _comprobantes.Agregar(viejo);
        _comprobantes.Agregar(deOtro);
        _comprobantes.Agregar(nuevo);

        var lista = _servicio.ListarPorEmisores([Cuit]);

        Assert.Equal(new[] { nuevo, viejo }, lista);
    }

    [Fact]
    public void ListarPorEmisores_SinEmisores_DevuelveVacio()
    {
        _comprobantes.Agregar(CrearComprobante());

        Assert.Empty(_servicio.ListarPorEmisores([]));
    }

    private Comprobante AgregarAutorizado()
    {
        var comprobante = CrearComprobante();
        comprobante.AplicarResultadoAutorizacion(new ResultadoAutorizacion
        {
            Autorizado = true,
            CAE = Cae,
            FechaVencimientoCAE = new DateOnly(2026, 11, 10)
        });
        _comprobantes.Agregar(comprobante);
        return comprobante;
    }

    private static Comprobante CrearComprobante(long numero = 1, DateTime? fecha = null) => new()
    {
        CUITEmisor = Cuit,
        Concepto = ConceptoComprobante.Productos,
        PuntoVentaNumero = 1,
        Tipo = TipoComprobante.FacturaA,
        Numero = numero,
        Fecha = fecha ?? new DateTime(2026, 10, 27),
        TipoDocumentoReceptor = TipoDocumento.CUIT,
        NumeroDocumentoReceptor = "30712345671",
        CondicionIvaReceptor = CondicionIva.ResponsableInscripto,
        ImporteNeto = 100000,
        ImporteIVA = 21000,
        ImporteTotal = 121000,
        Moneda = "PES",
        Cotizacion = 1
    };

    private static string ParametroP(string url) => url[(url.IndexOf("?p=", StringComparison.Ordinal) + 3)..];
}
