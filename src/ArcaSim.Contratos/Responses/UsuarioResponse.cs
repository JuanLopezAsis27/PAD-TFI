namespace ArcaSim.Contratos.Responses;

/// <summary>Datos de una cuenta de Clave Fiscal (nunca incluye la clave ni su hash).</summary>
public class UsuarioResponse
{
    public string Cuit { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; }
    public DateTime? UltimoIngreso { get; set; }

    /// <summary>CUIT de los contribuyentes que la cuenta puede operar.</summary>
    public List<string> CuitsRepresentados { get; set; } = new();
}
