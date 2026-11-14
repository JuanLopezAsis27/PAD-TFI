namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Datos para dar de alta un punto de venta asociado a un contribuyente.
/// </summary>
public class RegistrarPuntoVentaRequest
{
    public int Numero { get; set; } = 1;
    public bool Habilitado { get; set; } = true;
}

