namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Pedido para obtener la Clave Fiscal (persona humana). El nombre y el apellido se validan con RENAPER
/// contra el DNI de la CUIT/CUIL, así que tienen que estar escritos como en el DNI.
/// </summary>
public class RegistrarCuentaRequest
{
    /// <summary>CUIT/CUIL de 11 dígitos sin guiones (prefijo 20, 23, 24 o 27).</summary>
    public string Cuit { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Entre 10 y 64 caracteres, al menos 2 números, 4 letras, 1 mayúscula y 1 minúscula.</summary>
    public string Clave { get; set; } = string.Empty;
}
