
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Seguridad;

/// <summary>
/// Consulta las API keys SOAP registradas en PostgreSQL.
/// La búsqueda se realiza exclusivamente por hash SHA-256.
/// </summary>
public sealed class ConsultaApiKeysRepositorio : IConsultaApiKeys
{
    private readonly AppDbContext _context;

    public ConsultaApiKeysRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public ApiKeyRegistrada? ObtenerPorHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return null;
        }

        return _context.ApiKeysSoap
            .AsNoTracking()
            .Where(x => x.Hash == hash)
            .Select(x => new ApiKeyRegistrada(
                x.Cuit,
                x.Revocada))
            .FirstOrDefault();
    }
}
