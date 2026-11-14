namespace ArcaSim.Contratos.Responses;

/// <summary>
/// Respuesta al generar o regenerar la API Key de un contribuyente.
/// La clave en texto plano se devuelve una única vez en este momento.
/// </summary>
public class ApiKeyResponse
{
    public string Cuit { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTimeOffset FechaGeneracion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

