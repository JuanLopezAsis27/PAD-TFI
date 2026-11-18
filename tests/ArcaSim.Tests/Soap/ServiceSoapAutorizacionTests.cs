
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using System.Globalization;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class ServiceSoapAutorizacionTests
{
    private const string CuitEmisor = "30712345671";
    private const long CuitEmisorNumerica = 30712345671;
    private const int PuntoVentaNumero = 1;
    private const int CodigoFacturaB = 6;

    private static (
        ServiceSoap Servicio,
        ContribuyenteRepositorioFake Contribuyentes,
        PuntoVentaRepositorioFake PuntosVenta,
        ComprobanteRepositorioFake Comprobantes)
        CrearEntorno()
    {
        var contribuyentes = new ContribuyenteRepositorioFake(
            contribuyente => contribuyente.CUIT);

        var puntosVenta = new PuntoVentaRepositorioFake(
            punto => (punto.CUITContribuyente, punto.Numero));

        var comprobantes = new ComprobanteRepositorioFake();

        var comprobanteServicio = new ComprobanteServicio(
            contribuyentes,
            puntosVenta,
            comprobantes);

        return (
            new ServiceSoap(
                comprobanteServicio,
                new ValidadorApiKeyFake()),
            contribuyentes,
            puntosVenta,
            comprobantes);
    }

    private static FEAuthRequest CrearAuth() => new()
    {
        Token = "API_KEY_DE_PRUEBA",
        Sign = string.Empty,
        Cuit = CuitEmisorNumerica
    };

    private static FECAERequest CrearSolicitud() => new()
    {
        FeCabReq = new FECAECabRequest
        {
            CantReg = 1,
            PtoVta = PuntoVentaNumero,
            CbteTipo = CodigoFacturaB
        },
        FeDetReq = new List<FECAEDetRequest>
        {
            new()
            {
                Concepto = (int)ConceptoComprobante.Productos,
                DocTipo = (int)TipoDocumento.DNI,
                DocNro = 12345678,
                CbteDesde = 1,
                CbteHasta = 1,
                CbteFch = DateTime.Today.ToString(
                    "yyyyMMdd", CultureInfo.InvariantCulture),
                ImpTotal = 12100,
                ImpTotConc = 0,
                ImpNeto = 10000,
                ImpOpEx = 0,
                ImpTrib = 0,
                ImpIVA = 2100,
                MonId = "PES",
                MonCotiz = 1,

                // Código oficial ARCA para Consumidor Final.
                CondicionIVAReceptorId = 5,

                Iva = new List<FEAlicIva>
                {
                    new()
                    {
                        Id = (short)AlicuotaIva.GeneralVeintiuno,
                        BaseImp = 10000,
                        Importe = 2100
                    }
                }
            }
        }
    };

    private static Contribuyente RegistrarContribuyente(
        ContribuyenteRepositorioFake repositorio)
    {
        var contribuyente = new Contribuyente
        {
            CUIT = CuitEmisor,
            NombreORazonSocial = "Comercio de prueba",
            CondicionIva = CondicionIva.ResponsableInscripto,
            PuntosVenta = new()
        };

        repositorio.Agregar(contribuyente);
        return contribuyente;
    }

    private static PuntoVenta RegistrarPuntoVenta(
        PuntoVentaRepositorioFake repositorio,
        Contribuyente contribuyente)
    {
        var puntoVenta = new PuntoVenta
        {
            CUITContribuyente = CuitEmisor,
            Numero = PuntoVentaNumero,
            Habilitado = true,
            Contribuyente = contribuyente
        };

        contribuyente.PuntosVenta.Add(puntoVenta);
        repositorio.Agregar(puntoVenta);

        return puntoVenta;
    }

    private static Comprobante? ObtenerComprobante(
        ComprobanteRepositorioFake repositorio) =>
        repositorio.ObtenerPorIdentificacion(
            CuitEmisor,
            PuntoVentaNumero,
            TipoComprobante.FacturaB,
            1);

    [Fact]
    public void ContribuyenteInexistente_RechazaFactura()
    {
        var entorno = CrearEntorno();

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), CrearSolicitud());

        Assert.NotNull(respuesta.Errors);
        Assert.Contains(respuesta.Errors,
            error => error.Msg.Contains("DOM-012"));

        Assert.Equal("R", respuesta.FeCabResp?.Resultado);
        Assert.Null(ObtenerComprobante(entorno.Comprobantes));
    }

    [Fact]
    public void PuntoVentaInexistente_RechazaFactura()
    {
        var entorno = CrearEntorno();
        RegistrarContribuyente(entorno.Contribuyentes);

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), CrearSolicitud());

        Assert.NotNull(respuesta.Errors);
        Assert.Contains(respuesta.Errors,
            error => error.Msg.Contains("DOM-013"));

        Assert.Equal("R", respuesta.FeCabResp?.Resultado);
        Assert.Null(ObtenerComprobante(entorno.Comprobantes));
    }

    [Fact]
    public void FacturaRechazada_NoGeneraCae()
    {
        var entorno = CrearEntorno();
        RegistrarContribuyente(entorno.Contribuyentes);

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), CrearSolicitud());

        Assert.Equal("R", respuesta.FeCabResp?.Resultado);

        Assert.True(
            respuesta.FeDetResp is null ||
            respuesta.FeDetResp.All(
                d => d.Resultado != "A" &&
                     string.IsNullOrEmpty(d.CAE)));

        Assert.Null(ObtenerComprobante(entorno.Comprobantes));
    }

    [Fact]
    public void FacturaValida_GeneraCae()
    {
        var entorno = CrearEntorno();

        var contribuyente =
            RegistrarContribuyente(entorno.Contribuyentes);

        RegistrarPuntoVenta(entorno.PuntosVenta, contribuyente);

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), CrearSolicitud());

        Assert.Equal("A", respuesta.FeCabResp?.Resultado);

        Assert.True(
            respuesta.Errors is null ||
            respuesta.Errors.Count == 0);

        Assert.NotNull(respuesta.FeDetResp);
        var detalle = Assert.Single(respuesta.FeDetResp);

        Assert.Equal("A", detalle.Resultado);
        Assert.Matches(@"^\d{14}$", detalle.CAE);

        Assert.True(DateTime.TryParseExact(
            detalle.CAEFchVto,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _));

        var comprobante =
            ObtenerComprobante(entorno.Comprobantes);

        Assert.NotNull(comprobante);

        Assert.Equal(
            CondicionIva.ConsumidorFinal,
            comprobante.CondicionIvaReceptor);

        Assert.True(
            comprobante.EstadoAutorizacion is
                EstadoAutorizacionComprobante.Autorizado or
                EstadoAutorizacionComprobante.AutorizadoConObservaciones);

        Assert.Equal(detalle.CAE, comprobante.CAE);

        long ultimo =
            entorno.Comprobantes.ObtenerUltimoNumeroAutorizado(
                CuitEmisor,
                PuntoVentaNumero,
                TipoComprobante.FacturaB);

        Assert.Equal(1, ultimo);
    }

    [Fact]
    public void FacturaConImportesIncorrectos_NoGeneraCae()
    {
        var entorno = CrearEntorno();

        var contribuyente =
            RegistrarContribuyente(entorno.Contribuyentes);

        RegistrarPuntoVenta(entorno.PuntosVenta, contribuyente);

        var solicitud = CrearSolicitud();
        solicitud.FeDetReq[0].ImpTotal = 15000;

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), solicitud);

        Assert.Equal("R", respuesta.FeCabResp?.Resultado);
        Assert.NotNull(respuesta.Errors);

        Assert.Contains(
            respuesta.Errors,
            error => error.Msg.Contains("10048"));

        Assert.Null(ObtenerComprobante(entorno.Comprobantes));
    }

    [Fact]
    public void CondicionIvaNoSoportada_DevuelveError()
    {
        var entorno = CrearEntorno();
        var solicitud = CrearSolicitud();

        solicitud.FeDetReq[0].CondicionIVAReceptorId = 999;

        var respuesta = entorno.Servicio.FECAESolicitar(
            CrearAuth(), solicitud);

        Assert.NotNull(respuesta.Errors);
        Assert.Contains(respuesta.Errors, e => e.Code == 400);
    }

    private sealed class ValidadorApiKeyFake : IValidadorApiKey
    {
        public ResultadoValidacionApiKey Validar(
            string apiKey,
            string cuit)
        {
            return apiKey == "API_KEY_DE_PRUEBA" &&
                   cuit == CuitEmisor
                ? ResultadoValidacionApiKey.Exito()
                : ResultadoValidacionApiKey.Error(
                    600, "La API key no es válida.");
        }
    }
}
