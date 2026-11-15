
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Enums;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class FEParamGetTests
{
    private static ServiceSoap CrearServicio(bool autenticado = true)
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

    [Fact]
    public void TiposCbte_DevuelveTiposDelDominio()
    {
        var servicio = CrearServicio();

        var respuesta = servicio.FEParamGetTiposCbte(
            CrearAuth());

        Assert.NotNull(respuesta.ResultGet);
        Assert.NotEmpty(respuesta.ResultGet);

        Assert.Contains(
            respuesta.ResultGet,
            x => x.Id == (int)TipoComprobante.FacturaB);

        Assert.True(
            respuesta.Errors is null ||
            respuesta.Errors.Count == 0);
    }

    [Fact]
    public void TiposIva_DevuelveAlicuotasDelDominio()
    {
        var servicio = CrearServicio();

        var respuesta = servicio.FEParamGetTiposIva(
            CrearAuth());

        Assert.NotNull(respuesta.ResultGet);

        Assert.Contains(
            respuesta.ResultGet,
            x => x.Id == "5" && x.Desc == "21%");

        Assert.Contains(
            respuesta.ResultGet,
            x => x.Id == "4" && x.Desc == "10,5%");
    }

    [Fact]
    public void TiposCbte_SinAutenticacion_Devuelve600()
    {
        var servicio = CrearServicio(autenticado: false);

        var respuesta = servicio.FEParamGetTiposCbte(
            CrearAuth());

        Assert.Null(respuesta.ResultGet);
        Assert.NotNull(respuesta.Errors);
        Assert.Contains(respuesta.Errors, x => x.Code == 600);
    }

    [Fact]
    public void TiposIva_SinAutenticacion_Devuelve600()
    {
        var servicio = CrearServicio(autenticado: false);

        var respuesta = servicio.FEParamGetTiposIva(
            CrearAuth());

        Assert.Null(respuesta.ResultGet);
        Assert.NotNull(respuesta.Errors);
        Assert.Contains(respuesta.Errors, x => x.Code == 600);
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
