using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Integraciones.Renaper;
using ArcaSim.Infraestructura.Persistencia;
using ArcaSim.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArcaSim.Infraestructura;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura: base de datos, repositorios y el cliente de RENAPER.
    /// </summary>
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, string connectionString,
        RenaperOpciones renaper)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // Scoped, igual que el AppDbContext: todos los repositorios de un pedido comparten el mismo contexto.
        services.AddScoped<IContribuyenteRepositorio, ContribuyenteRepositorio>();
        services.AddScoped<IPuntoVentaRepositorio, PuntoVentaRepositorio>();
        services.AddScoped<IComprobanteRepositorio, ComprobanteRepositorio>();

        AddRenaper(services, renaper);

        return services;
    }

    private static void AddRenaper(IServiceCollection services, RenaperOpciones renaper)
    {
        if (renaper.Modo == ModoRenaper.Stub)
        {
            services.AddSingleton<IRenaperCliente>(new RenaperClienteSimulado(TimeProvider.System));
            return;
        }

        if (string.IsNullOrWhiteSpace(renaper.UrlBase) || string.IsNullOrWhiteSpace(renaper.ApiKey))
        {
            throw new InvalidOperationException(
                "Falta configurar Renaper:UrlBase y Renaper:ApiKey. En desarrollo se puede usar Renaper:Modo = Stub.");
        }

        services.AddHttpClient<IRenaperCliente, RenaperClienteHttp>(cliente =>
        {
            cliente.BaseAddress = new Uri(renaper.UrlBase.TrimEnd('/') + "/");
            cliente.Timeout = TimeSpan.FromSeconds(renaper.TimeoutSegundos);
            cliente.DefaultRequestHeaders.Add(RenaperClienteHttp.HeaderApiKey, renaper.ApiKey);
        });
    }
}
