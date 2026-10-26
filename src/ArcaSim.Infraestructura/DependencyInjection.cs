using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArcaSim.Infraestructura;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura (base de datos, repositorios, integraciones).
    /// TODO: registrar acá los repositorios y el cliente HTTP de RENAPER a medida que se implementen.
    /// </summary>
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
