namespace ArcaSim.Aplicacion.Interfaces;

public interface IConsultaApiKeys
{
    ApiKeyRegistrada? ObtenerPorHash(string hash);
}

public sealed record ApiKeyRegistrada(
    string Cuit,
    bool Revocada);
