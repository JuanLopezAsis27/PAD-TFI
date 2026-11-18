
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class ServiceSoapValidacionesTests
{
    private const long CuitPrueba = 30712345671;

    // =====================================================
    // MÉTODOS AUXILIARES
    // =====================================================

    private static FEAuthRequest CrearAuth(
        string token = "API_KEY_PRUEBA",
        string sign = "")
    {
        return new FEAuthRequest
        {
            Token = token,
            Sign = sign,
            Cuit = CuitPrueba
        };
    }

    private static FECAERequest CrearSolicitud()
    {
        return new FECAERequest
        {
            FeCabReq = new FECAECabRequest
            {
                CantReg = 1,
                PtoVta = 1,
                CbteTipo = 6
            },

            FeDetReq = new List<FECAEDetRequest>
            {
                new FECAEDetRequest
                {
                    Concepto = 1,
                    DocTipo = 96,
                    DocNro = 12345678,
                    CbteDesde = 1,
                    CbteHasta = 1,
                    CbteFch = "20261114",

                    ImpTotal = 12100,
                    ImpTotConc = 0,
                    ImpNeto = 10000,
                    ImpOpEx = 0,
                    ImpTrib = 0,
                    ImpIVA = 2100,

                    MonId = "PES",
                    MonCotiz = 1
                }
            }
        };
    }

    private static ServiceSoap CrearServicio(
        ResultadoValidacionApiKey? resultado = null)
    {
        var validador = new ValidadorApiKeyFake(
            resultado ?? ResultadoValidacionApiKey.Exito());

        // Estas pruebas se detienen antes de consultar
        // ComprobanteServicio. Por eso no requieren
        // repositorios ni base de datos.
        return new ServiceSoap(
            comprobanteServicio: null!,
            validadorApiKey: validador);
    }

    private static void VerificarError(
        FECAEResponse respuesta,
        int codigoEsperado)
    {
        Assert.NotNull(respuesta.Errors);
        Assert.NotEmpty(respuesta.Errors);

        Assert.Contains(
            respuesta.Errors,
            error => error.Code == codigoEsperado);

        Assert.True(
            respuesta.FeCabResp is null ||
            respuesta.FeCabResp.Resultado != "A");
    }

    // =====================================================
    // PRUEBA 1: AUTH NO INFORMADO
    // =====================================================

    [Fact]
    public void AuthNulo_DevuelveError600()
    {
        var servicio = CrearServicio();

        var respuesta = servicio.FECAESolicitar(
            null!,
            CrearSolicitud());

        VerificarError(respuesta, 600);
    }

    // =====================================================
    // PRUEBA 2: API KEY INVÁLIDA
    // =====================================================

    [Fact]
    public void ApiKeyInvalida_DevuelveError600()
    {
        var servicio = CrearServicio(
            ResultadoValidacionApiKey.Error(
                600,
                "La API key es inválida."));

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            CrearSolicitud());

        VerificarError(respuesta, 600);
    }

    // =====================================================
    // PRUEBA 3: CUIT INCORRECTA
    // =====================================================

    [Fact]
    public void CuitIncorrecta_DevuelveError601()
    {
        var servicio = CrearServicio(
            ResultadoValidacionApiKey.Error(
                601,
                "La CUIT no corresponde a la clave."));

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            CrearSolicitud());

        VerificarError(respuesta, 601);
    }

    // =====================================================
    // PRUEBA 4: WSAA NO IMPLEMENTADO
    // =====================================================

    [Fact]
    public void SignInformado_DevuelveError600()
    {
        var servicio = CrearServicio();

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(sign: "FIRMA_DIGITAL"),
            CrearSolicitud());

        VerificarError(respuesta, 600);
    }

    // =====================================================
    // PRUEBA 5: PUNTO DE VENTA INVÁLIDO
    // =====================================================

    [Fact]
    public void PuntoVentaInvalido_DevuelveError()
    {
        var servicio = CrearServicio();
        var solicitud = CrearSolicitud();

        solicitud.FeCabReq.PtoVta = 0;

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            solicitud);

        VerificarError(respuesta, 400);
    }

    // =====================================================
    // PRUEBA 6: TIPO DE COMPROBANTE INVÁLIDO
    // =====================================================

    [Fact]
    public void TipoComprobanteInvalido_DevuelveError()
    {
        var servicio = CrearServicio();
        var solicitud = CrearSolicitud();

        solicitud.FeCabReq.CbteTipo = -1;

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            solicitud);

        VerificarError(respuesta, 400);
    }

    // =====================================================
    // PRUEBA 7: VARIOS COMPROBANTES
    // =====================================================

    [Fact]
    public void VariosComprobantes_DevuelveError()
    {
        var servicio = CrearServicio();
        var solicitud = CrearSolicitud();

        solicitud.FeCabReq.CantReg = 2;

        solicitud.FeDetReq.Add(new FECAEDetRequest
        {
            Concepto = 1,
            DocTipo = 96,
            DocNro = 12345678,
            CbteDesde = 2,
            CbteHasta = 2,
            CbteFch = "20261114"
        });

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            solicitud);

        VerificarError(respuesta, 400);
    }

    // =====================================================
    // PRUEBA 8: NUMERACIÓN INVÁLIDA
    // =====================================================

    [Fact]
    public void NumeroComprobanteInvalido_DevuelveError()
    {
        var servicio = CrearServicio();
        var solicitud = CrearSolicitud();

        solicitud.FeDetReq[0].CbteDesde = 0;

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            solicitud);

        VerificarError(respuesta, 400);
    }

    // =====================================================
    // PRUEBA 9: FECHA INVÁLIDA
    // =====================================================

    [Fact]
    public void FechaInvalida_DevuelveError()
    {
        var servicio = CrearServicio();
        var solicitud = CrearSolicitud();

        solicitud.FeDetReq[0].CbteFch = "FECHA_INVALIDA";

        var respuesta = servicio.FECAESolicitar(
            CrearAuth(),
            solicitud);

        VerificarError(respuesta, 400);
    }

    // =====================================================
    // VALIDADOR FALSO PARA PRUEBAS
    // =====================================================

    private sealed class ValidadorApiKeyFake : IValidadorApiKey
    {
        private readonly ResultadoValidacionApiKey _resultado;

        public ValidadorApiKeyFake(
            ResultadoValidacionApiKey resultado)
        {
            _resultado = resultado;
        }

        public ResultadoValidacionApiKey Validar(
            string apiKey,
            string cuit)
        {
            return _resultado;
        }
    }
}
