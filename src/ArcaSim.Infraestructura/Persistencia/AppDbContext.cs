using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Persistencia;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL. El mapeo de cada entidad está en Configuraciones/.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Contribuyente> Contribuyentes => Set<Contribuyente>();
    public DbSet<PuntoVenta> PuntosVenta => Set<PuntoVenta>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
