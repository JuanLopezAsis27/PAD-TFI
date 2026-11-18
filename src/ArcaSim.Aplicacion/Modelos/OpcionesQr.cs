using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Modelos;

/// <summary>
/// Página que abre el código QR de un comprobante al escanearlo. En ARCA es
/// https://www.arca.gob.ar/fe/qr/; en ArcaSim tiene que ser la constatación del portal, porque ARCA real
/// no conoce los comprobantes del simulador.
/// </summary>
public sealed class OpcionesQr
{
    /// <summary>Ruta de la constatación por QR en el portal, la misma que usa ARCA.</summary>
    public const string RutaConstatacion = "fe/qr/";

    /// <summary>Apunta a ARCA real. Solo para cuando no se configuró la dirección del portal.</summary>
    public static OpcionesQr Arca { get; } = new(GeneradorQrComprobante.UrlBaseArca);

    /// <param name="urlBase">URL completa de la página de verificación, por ejemplo
    /// <c>https://portal/fe/qr/</c>.</param>
    /// <exception cref="ArgumentException">No es una URL absoluta http o https sin parámetros.</exception>
    public OpcionesQr(string urlBase)
    {
        if (!GeneradorQrComprobante.EsUrlBaseValida(urlBase))
        {
            throw new ArgumentException(
                $"La URL base del QR '{urlBase}' no es válida: tiene que ser absoluta (http o https) y sin parámetros.",
                nameof(urlBase));
        }

        UrlBase = urlBase;
    }

    /// <summary>URL a la que se le agrega <c>?p=...</c> con los datos del comprobante.</summary>
    public string UrlBase { get; }

    /// <summary>Indica si el QR lleva a ARCA real en vez de al portal.</summary>
    public bool ApuntaAArca => UrlBase == GeneradorQrComprobante.UrlBaseArca;

    /// <summary>Constatación del portal publicado en <paramref name="urlPublicaPortal"/> (su dirección de afuera).</summary>
    /// <exception cref="ArgumentException">La dirección no es válida.</exception>
    public static OpcionesQr ParaPortal(string urlPublicaPortal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(urlPublicaPortal);
        return new OpcionesQr($"{urlPublicaPortal.Trim().TrimEnd('/')}/{RutaConstatacion}");
    }
}
