using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;
using Microsoft.Extensions.Logging;

namespace ArcaSim.Infraestructura.Integraciones.Renaper;

/// <summary>
/// Cliente del servicio "Renaper Validation" del otro grupo: POST api/v1/personas/validar con la API key
/// en el header X-API-Key. La URL base, la API key y el timeout se configuran al registrar el HttpClient.
/// </summary>
public sealed class RenaperClienteHttp(HttpClient http, ILogger<RenaperClienteHttp> logger) : IRenaperCliente
{
    public const string HeaderApiKey = "X-API-Key";
    private const string RutaValidar = "api/v1/personas/validar";

    public async Task<ResultadoValidacionPersona> ValidarPersonaAsync(int dni, string nombre, string apellido,
        int edadMinima, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage respuesta;

        try
        {
            respuesta = await http.PostAsJsonAsync(RutaValidar,
                new SolicitudValidacion(dni, nombre, apellido, edadMinima), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "No se pudo conectar con RENAPER.");
            throw new RenaperNoDisponibleException("No se pudo conectar con RENAPER.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("RENAPER no respondió dentro del tiempo límite.");
            throw new RenaperNoDisponibleException("RENAPER no respondió a tiempo.", ex);
        }

        using (respuesta)
        {
            if (!respuesta.IsSuccessStatusCode)
            {
                logger.LogWarning("RENAPER respondió {Codigo} a una validación.", (int)respuesta.StatusCode);
                throw new RenaperNoDisponibleException(MensajeDeError(respuesta.StatusCode));
            }

            RespuestaValidacion? cuerpo;

            try
            {
                cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaValidacion>(cancellationToken);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                throw new RenaperNoDisponibleException("RENAPER devolvió una respuesta que no se pudo interpretar.", ex);
            }

            // La documentación del servicio pide no tomar como válida una respuesta con campos faltantes.
            if (cuerpo?.DatosCoinciden is not bool datosCoinciden || cuerpo.CumpleEdadMinima is not bool cumpleEdad)
                throw new RenaperNoDisponibleException("RENAPER devolvió una respuesta incompleta.");

            return new ResultadoValidacionPersona(datosCoinciden, cumpleEdad);
        }
    }

    private static string MensajeDeError(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
            "RENAPER rechazó la API key: revisar Renaper:ApiKey (quizás se generó una nueva).",
        HttpStatusCode.PaymentRequired =>
            "RENAPER no tiene saldo de consultas: hay que comprar créditos en su portal.",
        HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
            "RENAPER rechazó los datos enviados.",
        _ => $"RENAPER respondió con un error ({(int)codigo})."
    };

    private sealed record SolicitudValidacion(int Dni, string Nombre, string Apellido, int EdadMinima);

    private sealed record RespuestaValidacion(bool? DatosCoinciden, bool? CumpleEdadMinima);
}
