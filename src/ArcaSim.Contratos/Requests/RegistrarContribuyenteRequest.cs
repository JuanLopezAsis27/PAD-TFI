namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Alta de un contribuyente desde el portal, con la sesión de una cuenta de Clave Fiscal.
/// Persona humana: se inscribe la dueña de la cuenta, con el CUIT y el nombre ya validados con RENAPER
/// (se ignoran Cuit y RazonSocial). Persona jurídica: se envían Cuit y RazonSocial, y la cuenta queda
/// como su representante.
/// </summary>
public class RegistrarContribuyenteRequest
{
    /// <summary>"Humana" o "Juridica".</summary>
    public string TipoPersona { get; set; } = "Humana";

    /// <summary>CUIT de la persona jurídica (prefijo 30, 33 o 34).</summary>
    public string? Cuit { get; set; }

    /// <summary>Razón social (obligatoria para persona jurídica).</summary>
    public string? RazonSocial { get; set; }

    /// <summary>ID de condición frente al IVA según tabla de ARCA (1 = RI, 4 = Exento, 6 = Monotributista, etc.).</summary>
    public int CondicionIva { get; set; } = 1;
}
