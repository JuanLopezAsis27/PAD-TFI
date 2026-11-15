
namespace ArcaSim.Aplicacion.Interfaces;

public sealed record ApiKeySoapGenerada(
    int Id,
    string Cuit,
    string Clave,
    DateTime FechaCreacion);

public interface IAdministradorApiKeysSoap
{
    Task<ApiKeySoapGenerada> GenerarAsync(
        string cuit,
        CancellationToken cancellationToken = default);

    Task<bool> RevocarAsync(
        int id,
        string cuit,
        CancellationToken cancellationToken = default);
}
