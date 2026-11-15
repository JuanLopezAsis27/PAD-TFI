namespace ArcaSim.Dominio.Entidades;

/// <summary>
/// Relación entre una cuenta de Clave Fiscal y un contribuyente que puede operar.
/// Una persona humana se representa a sí misma cuando se inscribe; una persona jurídica no tiene
/// clave propia y la opera quien la dio de alta (en ARCA, el "Administrador de Relaciones").
/// </summary>
public class Representacion
{
    public string CuitUsuario { get; set; } = string.Empty;
    public string CuitRepresentado { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; }

    public Usuario? Usuario { get; set; }
    public Contribuyente? Representado { get; set; }
}
