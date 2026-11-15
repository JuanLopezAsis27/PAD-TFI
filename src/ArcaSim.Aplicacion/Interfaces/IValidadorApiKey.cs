namespace ArcaSim.Aplicacion.Interfaces;

public interface IValidadorApiKey
{
    ResultadoValidacionApiKey Validar(
        string apiKey,
        string cuit);
}

public record ResultadoValidacionApiKey(
    bool EsValida,
    int? CodigoError,
    string? Mensaje)
{
    public static ResultadoValidacionApiKey Exito() =>
        new(true, null, null);

    public static ResultadoValidacionApiKey Error(
        int codigo,
        string mensaje) =>
        new(false, codigo, mensaje);
}
