namespace ArcaSim.Infraestructura.Integraciones.Renaper;

/// <summary>Configuración de la sección "Renaper" de appsettings (ver docs/renaper.md).</summary>
public sealed class RenaperOpciones
{
    public const string Seccion = "Renaper";

    /// <summary>Stub: personas ficticias en memoria, sin red ni créditos. Http: el servicio del otro grupo.</summary>
    public ModoRenaper Modo { get; set; } = ModoRenaper.Http;

    public string UrlBase { get; set; } = "";

    /// <summary>Secreto: se configura con user-secrets o la variable de entorno Renaper__ApiKey, nunca en git.</summary>
    public string ApiKey { get; set; } = "";

    public int TimeoutSegundos { get; set; } = 10;
}

public enum ModoRenaper
{
    Stub,
    Http
}
