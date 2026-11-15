namespace ArcaSim.Contratos.Requests;

/// <summary>Ingreso con Clave Fiscal: CUIT/CUIL y clave.</summary>
public class IniciarSesionRequest
{
    public string Cuit { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}
