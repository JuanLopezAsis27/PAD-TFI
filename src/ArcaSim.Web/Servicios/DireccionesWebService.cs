namespace ArcaSim.Web.Servicios;

/// <summary>
/// Direcciones del web service SOAP tal como las ven los sistemas de afuera. Se muestran en la portada y en
/// la documentación (/documentacion).
/// </summary>
/// <remarks>
/// Salen de <c>Api:UrlPublica</c> o, si no está, de <c>Api:UrlBase</c>. Hace falta <c>Api:UrlPublica</c>
/// cuando el portal llama a la API por una dirección interna que los demás no ven (por ejemplo,
/// <c>http://api:8080</c> en Docker Compose).
/// </remarks>
public sealed class DireccionesWebService
{
    public DireccionesWebService(IConfiguration configuracion)
    {
        string? publica = configuracion["Api:UrlPublica"];
        string urlApi = string.IsNullOrWhiteSpace(publica)
            ? configuracion["Api:UrlBase"] ?? "http://localhost:5030"
            : publica;

        Servicio = urlApi.TrimEnd('/') + "/wsfev1/service.asmx";
    }

    /// <summary>Dirección a la que se envían los pedidos SOAP (POST).</summary>
    public string Servicio { get; }

    public string Wsdl => Servicio + "?WSDL";
}
