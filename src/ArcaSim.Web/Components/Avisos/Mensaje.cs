namespace ArcaSim.Web.Components.Avisos;

/// <summary>
/// Mensaje de confirmación o de error que muestra una página (con <see cref="AvisoMensaje"/>).
/// </summary>
/// <remarks>
/// Es una clase y no un string a propósito: <see cref="Aviso"/> compara por referencia, así que cada
/// mensaje nuevo se vuelve a mostrar aunque repita el texto del anterior (por ejemplo, el mismo error
/// de validación dos veces seguidas).
/// </remarks>
public sealed class Mensaje
{
    private Mensaje(string texto, bool esError)
    {
        Texto = texto;
        EsError = esError;
    }

    public string Texto { get; }

    public bool EsError { get; }

    public static Mensaje Exito(string texto) => new(texto, esError: false);

    public static Mensaje Error(string texto) => new(texto, esError: true);
}
