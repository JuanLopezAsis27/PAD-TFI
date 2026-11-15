
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Persistencia;
using ArcaSim.Infraestructura.Persistencia.Entidades;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ArcaSim.Infraestructura.Seguridad;

public sealed class AdministradorApiKeysSoap
    : IAdministradorApiKeysSoap
{
    private readonly AppDbContext _context;

    public AdministradorApiKeysSoap(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // GENERAR API KEY
    // =====================================================

    public async Task<ApiKeySoapGenerada> GenerarAsync(
        string cuit,
        CancellationToken cancellationToken = default)
    {
        // Validar formato de CUIT.
        if (string.IsNullOrWhiteSpace(cuit) ||
            cuit.Length != 11 ||
            !cuit.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                "La CUIT debe contener exactamente 11 dígitos.",
                nameof(cuit));
        }

        // Comprobar que el contribuyente exista.
        bool existeContribuyente = await _context.Contribuyentes
            .AsNoTracking()
            .AnyAsync(
                x => x.CUIT == cuit,
                cancellationToken);

        if (!existeContribuyente)
        {
            throw new InvalidOperationException(
                "No existe un contribuyente registrado " +
                "con la CUIT informada.");
        }

        // Generar 32 bytes aleatorios seguros.
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        // Convertir a una clave hexadecimal de 64 caracteres.
        string clave = Convert.ToHexString(bytes);

        // Calcular SHA-256 utilizando nuestro helper existente.
        string hash = GeneradorApiKey.CalcularHash(clave);

        DateTime fechaCreacion = DateTime.UtcNow;

        // Guardar únicamente el hash, nunca la clave original.
        var entidad = new ApiKeySoap
        {
            Hash = hash,
            Cuit = cuit,
            Revocada = false,
            FechaCreacion = fechaCreacion,
            FechaRevocacion = null
        };

        _context.ApiKeysSoap.Add(entidad);

        await _context.SaveChangesAsync(cancellationToken);

        // Entregar la clave original una única vez.
        return new ApiKeySoapGenerada(
            entidad.Id,
            entidad.Cuit,
            clave,
            fechaCreacion);
    }

    // =====================================================
    // REVOCAR API KEY
    // =====================================================

    public async Task<bool> RevocarAsync(
        int id,
        string cuit,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0 ||
            string.IsNullOrWhiteSpace(cuit))
        {
            return false;
        }

        DateTime fechaRevocacion = DateTime.UtcNow;

        // Actualización directa en PostgreSQL:
        // solo afecta la clave indicada, si sigue activa
        // y corresponde a la CUIT recibida.
        int filasActualizadas = await _context.ApiKeysSoap
            .Where(x =>
                x.Id == id &&
                x.Cuit == cuit &&
                !x.Revocada)
            .ExecuteUpdateAsync(
                actualizacion => actualizacion
                    .SetProperty(x => x.Revocada, true)
                    .SetProperty(
                        x => x.FechaRevocacion,
                        fechaRevocacion),
                cancellationToken);

        return filasActualizadas == 1;
    }
}
