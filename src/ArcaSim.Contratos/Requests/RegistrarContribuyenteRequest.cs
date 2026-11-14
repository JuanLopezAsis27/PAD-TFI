namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Datos para dar de alta un contribuyente desde el portal.
/// Si es persona humana, se envían Nombre y Apellido para validar con RENAPER.
/// Si es persona jurídica, se envía RazonSocial.
/// </summary>
public class RegistrarContribuyenteRequest
{
    public string Cuit { get; set; } = string.Empty;

    /// <summary>"Humana" o "Juridica".</summary>
    public string TipoPersona { get; set; } = "Humana";

    /// <summary>Nombre de pila (obligatorio para persona humana).</summary>
    public string? Nombre { get; set; }

    /// <summary>Apellido (obligatorio para persona humana).</summary>
    public string? Apellido { get; set; }

    /// <summary>Razón social (obligatorio para persona jurídica).</summary>
    public string? RazonSocial { get; set; }

    /// <summary>ID de condición frente al IVA según tabla de ARCA (1 = RI, 4 = Exento, 6 = Monotributista, etc.).</summary>
    public int CondicionIva { get; set; } = 1;
}

