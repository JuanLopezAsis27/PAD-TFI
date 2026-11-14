namespace ArcaSim.Contratos.Requests;

/// <summary>
/// Pedido para constatar públicamente un comprobante.
/// Puede verificarse pegando la URL del código QR (<see cref="UrlQr"/>)
/// o completando los datos del comprobante y su CAE.
/// </summary>
public class ConstatarComprobanteRequest
{
    /// <summary>Si se envía la URL del QR de ARCA, se valida directamente decodificando su parámetro 'p'.</summary>
    public string? UrlQr { get; set; }

    public string? CuitEmisor { get; set; }
    public int PuntoVentaNumero { get; set; } = 1;
    public int TipoComprobante { get; set; } = 1;
    public long Numero { get; set; } = 1;
    public string? Cae { get; set; }
}

