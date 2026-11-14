using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Seguridad;
using Xunit;

namespace ArcaSim.Tests.Seguridad;

public class ValidadorApiKeyTests
{
    private const string CuitCorrecta = "30712345671";
    private const string CuitIncorrecta = "30999999999";

    private readonly string _apiKey;
    private readonly ConsultaApiKeysFake _consulta;
    private readonly ValidadorApiKey _validador;

    public ValidadorApiKeyTests()
    {
        _apiKey = GeneradorApiKey.Generar();

        _consulta = new ConsultaApiKeysFake();

        _validador = new ValidadorApiKey(_consulta);
    }

    [Fact]
    public void ApiKeyVacia_DevuelveError600()
    {
        var resultado = _validador.Validar(
            "",
            CuitCorrecta);

        Assert.False(resultado.EsValida);
        Assert.Equal(600, resultado.CodigoError);
    }

    [Fact]
    public void ApiKeyInexistente_DevuelveError600()
    {
        var resultado = _validador.Validar(
            _apiKey,
            CuitCorrecta);

        Assert.False(resultado.EsValida);
        Assert.Equal(600, resultado.CodigoError);
    }

    [Fact]
    public void ApiKeyRevocada_DevuelveError600()
    {
        _consulta.Registrar(
            _apiKey,
            CuitCorrecta,
            revocada: true);

        var resultado = _validador.Validar(
            _apiKey,
            CuitCorrecta);

        Assert.False(resultado.EsValida);
        Assert.Equal(600, resultado.CodigoError);
    }

    [Fact]
    public void CuitIncorrecta_DevuelveError601()
    {
        _consulta.Registrar(
            _apiKey,
            CuitCorrecta,
            revocada: false);

        var resultado = _validador.Validar(
            _apiKey,
            CuitIncorrecta);

        Assert.False(resultado.EsValida);
        Assert.Equal(601, resultado.CodigoError);
    }

    [Fact]
    public void ApiKeyValida_DevuelveExito()
    {
        _consulta.Registrar(
            _apiKey,
            CuitCorrecta,
            revocada: false);

        var resultado = _validador.Validar(
            _apiKey,
            CuitCorrecta);

        Assert.True(resultado.EsValida);
        Assert.Null(resultado.CodigoError);
    }

    // Implementación falsa, usada solamente en las pruebas.
    private sealed class ConsultaApiKeysFake : IConsultaApiKeys
    {
        private readonly Dictionary<string, ApiKeyRegistrada>
            _claves = new();

        public void Registrar(
            string apiKey,
            string cuit,
            bool revocada)
        {
            string hash = GeneradorApiKey.CalcularHash(apiKey);

            _claves[hash] = new ApiKeyRegistrada(
                cuit,
                revocada);
        }

        public ApiKeyRegistrada? ObtenerPorHash(string hash)
        {
            return _claves.GetValueOrDefault(hash);
        }
    }
}
