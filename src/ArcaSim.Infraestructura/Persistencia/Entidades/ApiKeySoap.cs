
namespace ArcaSim.Infraestructura.Persistencia.Entidades;

public sealed class ApiKeySoap
{
    public int Id { get; set; }

    // Hash SHA-256 en formato hexadecimal.
    // Nunca almacenar la API key original.
    public string Hash { get; set; } = string.Empty;

    // CUIT del contribuyente propietario de la clave.
    public string Cuit { get; set; } = string.Empty;

    // Permite invalidar una clave sin eliminarla.
    public bool Revocada { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaRevocacion { get; set; }
}
