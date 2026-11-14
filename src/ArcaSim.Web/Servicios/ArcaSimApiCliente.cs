using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;

namespace ArcaSim.Web.Servicios;

/// <summary>
/// Cliente HTTP del portal Blazor para comunicarse con ArcaSim.Api (REST/JSON).
/// </summary>
public class ArcaSimApiCliente(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
        return await http.GetFromJsonAsync<List<ContribuyenteResponse>>("api/contribuyentes", JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<ContribuyenteResponse?> ObtenerContribuyenteAsync(string cuit, CancellationToken cancellationToken = default)
    {
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

    private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
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

