
using ArcaSim.Dominio.Entidades;
using ArcaSim.Infraestructura.Persistencia.Entidades;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Persistencia;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL.
/// El mapeo de cada entidad está en Configuraciones/.
/// </summary>
public class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    // Entidades del Rol 3.
    public DbSet<Contribuyente> Contribuyentes =>
        Set<Contribuyente>();

    public DbSet<PuntoVenta> PuntosVenta =>
        Set<PuntoVenta>();

    public DbSet<Comprobante> Comprobantes =>
        Set<Comprobante>();

    // Seguridad SOAP del Rol 2.
    public DbSet<ApiKeySoap> ApiKeysSoap =>
        Set<ApiKeySoap>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
