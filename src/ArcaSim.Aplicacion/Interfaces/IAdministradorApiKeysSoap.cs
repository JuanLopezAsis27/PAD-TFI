
namespace ArcaSim.Aplicacion.Interfaces;

public sealed record ApiKeySoapGenerada(
    int Id,
    string Cuit,
    string Clave,
    DateTime FechaCreacion);

/// <summary>Resultado de regenerar: la clave nueva y cuántas claves activas se revocaron.</summary>
public sealed record ApiKeySoapRegenerada(
    ApiKeySoapGenerada Nueva,
    int Revocadas);

public interface IAdministradorApiKeysSoap
{
    Task<ApiKeySoapGenerada> GenerarAsync(
        string cuit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca las API keys activas de la CUIT y genera una nueva, en una sola transacción.
    /// Es lo que hace el portal al generar o regenerar la API key: la anterior deja de funcionar.
    /// </summary>
    Task<ApiKeySoapRegenerada> RegenerarAsync(
        string cuit,
        CancellationToken cancellationToken = default);

    Task<bool> RevocarAsync(
        int id,
        string cuit,
        CancellationToken cancellationToken = default);
}
