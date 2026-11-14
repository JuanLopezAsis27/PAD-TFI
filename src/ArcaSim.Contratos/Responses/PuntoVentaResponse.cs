namespace ArcaSim.Contratos.Responses;

public class PuntoVentaResponse
{
    public string CuitContribuyente { get; set; } = string.Empty;
    public int Numero { get; set; }
    public bool Habilitado { get; set; }
}

