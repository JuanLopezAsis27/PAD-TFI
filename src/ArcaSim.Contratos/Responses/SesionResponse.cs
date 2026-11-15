namespace ArcaSim.Contratos.Responses;

/// <summary>
/// Respuesta a un ingreso correcto. El token (JWT) se envía en el header <c>Authorization: Bearer</c>
/// de las llamadas a la API REST del portal.
/// </summary>
public class SesionResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset Expira { get; set; }
    public UsuarioResponse Usuario { get; set; } = new();
}
