
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Enums;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class FEParamCondicionIvaTests
{
    private static ServiceSoap CrearServicio(
        bool autenticado = true)
    {
        return new ServiceSoap(
            comprobanteServicio: null!,
            validadorApiKey: new ValidadorFake(autenticado));
    }

    private static FEAuthRequest CrearAuth() => new()
    {
        Token = "CLAVE_PRUEBA",
        Sign = string.Empty,
        Cuit = 30712345671
    };

    [Theory]
    [InlineData(1, CondicionIva.ResponsableInscripto)]
    [InlineData(4, CondicionIva.Exento)]
    [InlineData(5, CondicionIva.ConsumidorFinal)]
    [InlineData(6, CondicionIva.Monotributista)]
    [InlineData(7, CondicionIva.SujetoNoCategorizado)]
    public void CodigoArca_SeConvierteAlEnumCorrecto(
        int codigo,
        CondicionIva esperado)
    {
        bool convertido =
            CondicionIvaArcaMapper.TryObtenerCondicion(
                codigo,
                out var obtenido);

        Assert.True(convertido);
        Assert.Equal(esperado, obtenido);
    }

    [Fact]
    public void ClaseB_DevuelveCondicionesCorrespondientes()
    {
        var servicio = CrearServicio();

        var respuesta =
            servicio.FEParamGetCondicionIvaReceptor(
                CrearAuth(), "B");

        Assert.NotNull(respuesta.ResultGet);
        Assert.NotEmpty(respuesta.ResultGet);
        Assert.All(
            respuesta.ResultGet,
            item => Assert.Equal("B", item.CmpClase));

        Assert.Contains(
            respuesta.ResultGet,
            item => item.Id == 5 &&
                    item.Desc == "Consumidor Final");

        Assert.True(
            respuesta.Errors is null ||
            respuesta.Errors.Count == 0);
    }

    [Fact]
    public void SinClase_DevuelveVariasClases()
    {
        var servicio = CrearServicio();

        var respuesta =
            servicio.FEParamGetCondicionIvaReceptor(
                CrearAuth(), "");

        Assert.NotNull(respuesta.ResultGet);

        Assert.Contains(
            respuesta.ResultGet,
            item => item.CmpClase == "A");

        Assert.Contains(
            respuesta.ResultGet,
            item => item.CmpClase == "B");

        Assert.Contains(
            respuesta.ResultGet,
            item => item.CmpClase == "C");
    }

    [Fact]
    public void ClaseInvalida_DevuelveError10244()
    {
        var servicio = CrearServicio();

        var respuesta =
            servicio.FEParamGetCondicionIvaReceptor(
                CrearAuth(), "Z");

        Assert.Null(respuesta.ResultGet);
        Assert.NotNull(respuesta.Errors);

        Assert.Contains(
            respuesta.Errors,
            item => item.Code == 10244);
    }

    [Fact]
    public void ApiKeyInvalida_DevuelveError600()
    {
        var servicio = CrearServicio(autenticado: false);

        var respuesta =
            servicio.FEParamGetCondicionIvaReceptor(
                CrearAuth(), "B");

        Assert.Null(respuesta.ResultGet);
        Assert.NotNull(respuesta.Errors);

        Assert.Contains(
            respuesta.Errors,
            item => item.Code == 600);
    }

    [Fact]
    public void CodigoDesconocido_NoSeMapea()
    {
        bool convertido =
            CondicionIvaArcaMapper.TryObtenerCondicion(
                999,
                out _);

        Assert.False(convertido);
    }

    private sealed class ValidadorFake : IValidadorApiKey
    {
        private readonly bool _autenticado;

        public ValidadorFake(bool autenticado)
        {
            _autenticado = autenticado;
        }

        public ResultadoValidacionApiKey Validar(
            string apiKey,
            string cuit)
        {
            return _autenticado
                ? ResultadoValidacionApiKey.Exito()
                : ResultadoValidacionApiKey.Error(
                    600, "API key inválida.");
        }
    }
}
