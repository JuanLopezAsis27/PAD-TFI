using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArcaSim.Infraestructura.Persistencia;

public static class InicializadorBaseDatos
{
    /// <summary>
    /// Aplica las migraciones pendientes y carga los datos de prueba, según se pida.
    /// La API lo llama al iniciar con los valores de BaseDeDatos:AplicarMigraciones y
    /// BaseDeDatos:CargarDatosDePrueba de la configuración del ambiente.
    /// </summary>
    public static void InicializarBaseDeDatos(this IServiceProvider services, bool aplicarMigraciones,
        bool cargarDatosDePrueba)
    {
        if (!aplicarMigraciones && !cargarDatosDePrueba)
            return;

        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("ArcaSim.BaseDeDatos")
            ?? NullLogger.Instance;

        try
        {
            if (aplicarMigraciones)
            {
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
                logger.LogInformation("Migraciones de la base de datos aplicadas.");
            }

            if (cargarDatosDePrueba)
            {
                int cargados = DatosDePrueba.Cargar(scope.ServiceProvider);
                logger.LogInformation("Datos de prueba: {Cantidad} contribuyentes nuevos.", cargados);
            }
        }
        catch (Exception ex) when (ex.GetBaseException() is System.Net.Sockets.SocketException)
        {
            logger.LogCritical("No se pudo conectar a PostgreSQL. Si estás en desarrollo, levantalo con " +
                "'docker compose up -d' y volvé a iniciar la API.");
            throw;
        }
    }
}
