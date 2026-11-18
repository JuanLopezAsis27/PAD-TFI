using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Dominio.Reglas;

public class ServicioQrComprobante
{
    /// <summary>URL del QR de un comprobante autorizado. Ver <see cref="GeneradorQrComprobante.GenerarUrl"/>.</summary>
    public static string GenerarUrl(Comprobante? comprobante, string urlBase)
    {
        var datosQr = MapeadorDatosQrComprobante.Mapear(comprobante);
        return GeneradorQrComprobante.GenerarUrl(datosQr, urlBase);
    }
}
