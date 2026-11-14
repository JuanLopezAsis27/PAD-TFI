namespace ArcaSim.Contratos.Responses;

public class ConstatacionResponse
{
    public bool EsValido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public ComprobanteResponse? Comprobante { get; set; }
}

