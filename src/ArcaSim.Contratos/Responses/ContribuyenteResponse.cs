namespace ArcaSim.Contratos.Responses;

public class ContribuyenteResponse
{
    public string Cuit { get; set; } = string.Empty;
    public string NombreORazonSocial { get; set; } = string.Empty;
    public string TipoPersona { get; set; } = string.Empty;
    public int CondicionIva { get; set; }
    public string CondicionIvaDescripcion { get; set; } = string.Empty;
    public List<PuntoVentaResponse> PuntosVenta { get; set; } = new();
}

