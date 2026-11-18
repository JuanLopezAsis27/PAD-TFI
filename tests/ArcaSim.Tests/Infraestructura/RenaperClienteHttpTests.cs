using System.Net;
using System.Text;
using System.Text.Json;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Infraestructura.Integraciones.Renaper;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArcaSim.Tests.Infraestructura;

// Se prueba contra un servidor falso: cada consulta al servicio real consume un crédito.
public class RenaperClienteHttpTests
{
    private sealed class ServidorFalso(Func<CancellationToken, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? Pedido { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Pedido = request;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return responder(cancellationToken);
        }
    }

    private static (RenaperClienteHttp Cliente, ServidorFalso Servidor) Crear(
        Func<CancellationToken, HttpResponseMessage> responder)
    {
        var servidor = new ServidorFalso(responder);
        var http = new HttpClient(servidor) { BaseAddress = new Uri("https://renaper.test/") };
        http.DefaultRequestHeaders.Add(RenaperClienteHttp.HeaderApiKey, "rnp_test_clave");
        return (new RenaperClienteHttp(http, NullLogger<RenaperClienteHttp>.Instance), servidor);
    }

    private static (RenaperClienteHttp Cliente, ServidorFalso Servidor) Crear(HttpStatusCode codigo, string cuerpo) =>
        Crear(_ => new HttpResponseMessage(codigo)
        {
            Content = new StringContent(cuerpo, Encoding.UTF8, "application/json")
        });

    private static Task<ArcaSim.Aplicacion.Modelos.ResultadoValidacionPersona> Validar(RenaperClienteHttp cliente,
        CancellationToken cancellationToken = default) =>
        cliente.ValidarPersonaAsync(40111222, "Lucas Matias", "Gomez", 18, cancellationToken);

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RespuestaCorrecta_DevuelveLosDosValores(bool datosCoinciden, bool cumpleEdad)
    {
        var (cliente, _) = Crear(HttpStatusCode.OK,
            $$"""{"datosCoinciden": {{datosCoinciden.ToString().ToLowerInvariant()}}, "cumpleEdadMinima": {{cumpleEdad.ToString().ToLowerInvariant()}}}""");

        var resultado = await Validar(cliente);

        Assert.Equal(datosCoinciden, resultado.DatosCoinciden);
        Assert.Equal(cumpleEdad, resultado.CumpleEdadMinima);
    }

    [Fact]
    public async Task EnviaElPedidoComoPideLaDocumentacion()
    {
        var (cliente, servidor) = Crear(HttpStatusCode.OK, """{"datosCoinciden": true, "cumpleEdadMinima": true}""");

        await Validar(cliente);

        Assert.Equal(HttpMethod.Post, servidor.Pedido!.Method);
        Assert.Equal("https://renaper.test/api/v1/personas/validar", servidor.Pedido.RequestUri!.ToString());
        Assert.Equal("rnp_test_clave", Assert.Single(servidor.Pedido.Headers.GetValues("X-API-Key")));
        Assert.Equal("application/json", servidor.Pedido.Content!.Headers.ContentType!.MediaType);

        using var json = JsonDocument.Parse(servidor.Cuerpo!);
        var raiz = json.RootElement;
        Assert.Equal(JsonValueKind.Number, raiz.GetProperty("dni").ValueKind);
        Assert.Equal(40111222, raiz.GetProperty("dni").GetInt32());
        Assert.Equal("Lucas Matias", raiz.GetProperty("nombre").GetString());
        Assert.Equal("Gomez", raiz.GetProperty("apellido").GetString());
        Assert.Equal(18, raiz.GetProperty("edadMinima").GetInt32());
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "API key")]
    [InlineData(HttpStatusCode.Forbidden, "API key")]
    [InlineData(HttpStatusCode.PaymentRequired, "saldo")]
    [InlineData(HttpStatusCode.BadRequest, "rechazó los datos")]
    [InlineData(HttpStatusCode.InternalServerError, "(500)")]
    public async Task RespuestaDeError_LanzaRenaperNoDisponibleConMensajeClaro(HttpStatusCode codigo, string mensaje)
    {
        var (cliente, _) = Crear(codigo, "");

        var ex = await Assert.ThrowsAsync<RenaperNoDisponibleException>(() => Validar(cliente));

        Assert.Contains(mensaje, ex.Message);
    }

    [Theory]
    [InlineData("""{"datosCoinciden": true}""")]
    [InlineData("""{"datosCoinciden": "true", "cumpleEdadMinima": true}""")]
    [InlineData("""{}""")]
    [InlineData("no es json")]
    [InlineData("")]
    public async Task RespuestaIncompletaOInvalida_NoSeTomaComoValida(string cuerpo)
    {
        var (cliente, _) = Crear(HttpStatusCode.OK, cuerpo);

        await Assert.ThrowsAsync<RenaperNoDisponibleException>(() => Validar(cliente));
    }

    [Fact]
    public async Task SinConexion_LanzaRenaperNoDisponible()
    {
        var (cliente, _) = Crear(_ => throw new HttpRequestException("Connection refused"));

        var ex = await Assert.ThrowsAsync<RenaperNoDisponibleException>(() => Validar(cliente));

        Assert.Contains("conectar", ex.Message);
    }

    [Fact]
    public async Task SinRespuestaATiempo_LanzaRenaperNoDisponible()
    {
        var (cliente, _) = Crear(_ => throw new TaskCanceledException("Timeout"));

        var ex = await Assert.ThrowsAsync<RenaperNoDisponibleException>(() => Validar(cliente));

        Assert.Contains("a tiempo", ex.Message);
    }

    [Fact]
    public async Task CancelacionDeQuienLlama_NoSeConfundeConFallaDeRenaper()
    {
        using var cancelacion = new CancellationTokenSource();
        cancelacion.Cancel();
        var (cliente, _) = Crear(token =>
        {
            token.ThrowIfCancellationRequested();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Validar(cliente, cancelacion.Token));
    }
}
