using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using Microsoft.AspNetCore.Components.Authorization;

namespace ArcaSim.Web.Servicios;

/// <summary>
/// Cliente HTTP del portal Blazor para comunicarse con ArcaSim.Api (REST/JSON).
/// Si hay una sesión iniciada, manda su JWT en <c>Authorization: Bearer</c>.
/// </summary>
public class ArcaSimApiCliente(HttpClient http, AuthenticationStateProvider estadoAutenticacion)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ===== Clave Fiscal (públicos: no llevan token) =====

    public async Task<(UsuarioResponse? Datos, string? Error)> RegistrarCuentaAsync(
        RegistrarCuentaRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/cuenta/registro", request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var creada = await response.Content.ReadFromJsonAsync<UsuarioResponse>(JsonOptions, cancellationToken);
            return (creada, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    public async Task<(SesionResponse? Sesion, string? Error)> IniciarSesionAsync(
        IniciarSesionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/cuenta/login", request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var sesion = await response.Content.ReadFromJsonAsync<SesionResponse>(JsonOptions, cancellationToken);
            return (sesion, null);
        }

        // 401 acá es "CUIT o clave incorrectos", no una sesión vencida: se muestra el mensaje de la API.
        return (null, await LeerMensajeErrorAsync(response, cancellationToken, esIngreso: true));
    }

    public async Task<UsuarioResponse?> ObtenerCuentaAsync(CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        using var response = await http.GetAsync("api/cuenta", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UsuarioResponse>(JsonOptions, cancellationToken);
    }

    /// <summary>Pone el JWT de la sesión (guardado en la cookie del portal) en las llamadas de este cliente.</summary>
    private async Task AgregarTokenAsync()
    {
        var estado = await estadoAutenticacion.GetAuthenticationStateAsync();
        var token = estado.User.FindFirst(ClaimsSesion.Token)?.Value;

        http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    // ===== Portal (requieren sesión) =====

    public async Task<bool> VerificarEstadoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("api/estado", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ContribuyenteResponse>> ListarContribuyentesAsync(CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        return await http.GetFromJsonAsync<List<ContribuyenteResponse>>("api/contribuyentes", JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<ContribuyenteResponse?> ObtenerContribuyenteAsync(string cuit, CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        using var response = await http.GetAsync($"api/contribuyentes/{Uri.EscapeDataString(cuit.Trim())}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContribuyenteResponse>(JsonOptions, cancellationToken);
    }

    public async Task<(ContribuyenteResponse? Datos, string? Error)> RegistrarContribuyenteAsync(
        RegistrarContribuyenteRequest request,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        using var response = await http.PostAsJsonAsync("api/contribuyentes", request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var creado = await response.Content.ReadFromJsonAsync<ContribuyenteResponse>(JsonOptions, cancellationToken);
            return (creado, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    public async Task<(PuntoVentaResponse? Datos, string? Error)> RegistrarPuntoVentaAsync(
        string cuit,
        RegistrarPuntoVentaRequest request,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        var url = $"api/contribuyentes/{Uri.EscapeDataString(cuit.Trim())}/puntos-venta";
        using var response = await http.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var creado = await response.Content.ReadFromJsonAsync<PuntoVentaResponse>(JsonOptions, cancellationToken);
            return (creado, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    public async Task<(ApiKeyResponse? Datos, string? Error)> GenerarApiKeyAsync(
        string cuit,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        var url = $"api/contribuyentes/{Uri.EscapeDataString(cuit.Trim())}/api-key";
        using var response = await http.PostAsync(url, content: null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var apiKey = await response.Content.ReadFromJsonAsync<ApiKeyResponse>(JsonOptions, cancellationToken);
            return (apiKey, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    public async Task<List<ComprobanteResponse>> ListarComprobantesAsync(
        string? cuit = null,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        var url = string.IsNullOrWhiteSpace(cuit)
            ? "api/comprobantes"
            : $"api/comprobantes?cuit={Uri.EscapeDataString(cuit.Trim())}";

        return await http.GetFromJsonAsync<List<ComprobanteResponse>>(url, JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<ComprobanteResponse?> ObtenerComprobanteAsync(
        string cuit,
        int puntoVenta,
        int tipo,
        long numero,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        var url = $"api/comprobantes/{Uri.EscapeDataString(cuit.Trim())}/{puntoVenta}/{tipo}/{numero}";
        using var response = await http.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ComprobanteResponse>(JsonOptions, cancellationToken);
    }

    public async Task<long> ObtenerProximoNumeroAsync(
        string cuit,
        int puntoVenta,
        int tipo,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        try
        {
            var url = $"api/comprobantes/ultimo-autorizado?cuit={Uri.EscapeDataString(cuit.Trim())}&puntoVenta={puntoVenta}&tipo={tipo}";
            using var doc = await http.GetFromJsonAsync<JsonDocument>(url, cancellationToken);
            if (doc is not null && doc.RootElement.TryGetProperty("proximoNumero", out var prop))
                return prop.GetInt64();
        }
        catch
        {
            // Si falla la consulta preventiva, se devuelve 1 por defecto.
        }

        return 1;
    }

    public async Task<(ComprobanteResponse? Comprobante, string? ErrorGeneral)> EmitirComprobanteAsync(
        EmitirComprobanteRequest request,
        CancellationToken cancellationToken = default)
    {
        await AgregarTokenAsync();
        using var response = await http.PostAsJsonAsync("api/comprobantes", request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var resultado = await response.Content.ReadFromJsonAsync<ComprobanteResponse>(JsonOptions, cancellationToken);
            return (resultado, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    public async Task<(ConstatacionResponse? Resultado, string? Error)> ConstatarComprobanteAsync(
        ConstatarComprobanteRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/constatacion", request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var resultado = await response.Content.ReadFromJsonAsync<ConstatacionResponse>(JsonOptions, cancellationToken);
            return (resultado, null);
        }

        return (null, await LeerMensajeErrorAsync(response, cancellationToken));
    }

    private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response,
        CancellationToken cancellationToken, bool esIngreso = false)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized && !esIngreso)
            return "Tu sesión expiró o no es válida. Volvé a ingresar con tu Clave Fiscal.";

        try
        {
            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
            if (doc is not null && doc.RootElement.TryGetProperty("mensaje", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                return msg.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
            }
        }
        catch
        {
            // Ignorar si no es un JSON con propiedad "mensaje".
        }

        return $"Error al comunicarse con la API (código {(int)response.StatusCode}).";
    }
}

