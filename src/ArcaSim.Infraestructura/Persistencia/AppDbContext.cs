using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Persistencia;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL.
/// TODO: agregar los DbSet de las entidades del dominio (Comprobante, PuntoVenta, Contribuyente, ...)
/// y sus configuraciones en la carpeta Configuraciones/.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
