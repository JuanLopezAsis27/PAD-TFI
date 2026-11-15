namespace ArcaSim.Dominio.Entidades;

/// <summary>
/// Cuenta de Clave Fiscal. Como en ARCA, la tiene una persona humana y su usuario es la CUIT/CUIL.
/// El nombre y el apellido son los que confirmó RENAPER al crear la cuenta.
/// Tener Clave Fiscal no te hace contribuyente: la inscripción es un trámite aparte (ver
/// <see cref="Representacion"/>).
/// </summary>
public class Usuario
{
    public string Cuit { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash de la clave (nunca la clave en texto plano).</summary>
    public string HashClave { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; }
    public DateTime? UltimoIngreso { get; set; }

    /// <summary>Intentos fallidos seguidos. Se reinicia con un ingreso correcto.</summary>
    public int IntentosFallidos { get; set; }

    /// <summary>Si tiene valor y todavía no pasó, la clave está bloqueada por intentos fallidos.</summary>
    public DateTime? BloqueadoHasta { get; set; }

    /// <summary>Contribuyentes que esta persona puede operar ("Representar a…" en ARCA).</summary>
    public List<Representacion> Representaciones { get; set; } = new();

    /// <summary>Nombre como lo muestra ARCA: "Apellido, Nombre".</summary>
    public string NombreCompleto => $"{Apellido}, {Nombre}";
}
