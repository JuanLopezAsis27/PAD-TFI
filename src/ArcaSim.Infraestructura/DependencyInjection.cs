
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Integraciones.Renaper;
using ArcaSim.Infraestructura.Persistencia;
using ArcaSim.Infraestructura.Repositorios;
using ArcaSim.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArcaSim.Infraestructura;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura:
    /// base de datos, repositorios, seguridad SOAP
    /// y el cliente de RENAPER.
    /// </summary>
    public static IServiceCollection AddInfraestructura(
        this IServiceCollection services,
        string connectionString,
        RenaperOpciones renaper)
    {
        // =================================================
        // BASE DE DATOS
        // =================================================

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // =================================================
        // REPOSITORIOS DEL ROL 3
        // =================================================

        services.AddScoped<
            IContribuyenteRepositorio,
            ContribuyenteRepositorio>();

        services.AddScoped<
            IPuntoVentaRepositorio,
            PuntoVentaRepositorio>();

        services.AddScoped<
            IComprobanteRepositorio,
            ComprobanteRepositorio>();

        // =================================================
        // SEGURIDAD SOAP DEL ROL 2
        // =================================================

        services.AddScoped<
            IConsultaApiKeys,
            ConsultaApiKeysRepositorio>();

        services.AddScoped<
            IValidadorApiKey,
            ValidadorApiKey>();

        services.AddScoped<
            IAdministradorApiKeysSoap,
            AdministradorApiKeysSoap>();

        // =================================================
        // INTEGRACIÓN RENAPER
        // =================================================

        AddRenaper(services, renaper);

        return services;
    }

    private static void AddRenaper(
        IServiceCollection services,
        RenaperOpciones renaper)
    {
        if (renaper.Modo == ModoRenaper.Stub)
        {
            services.AddSingleton<IRenaperCliente>(
                new RenaperClienteSimulado(
                    TimeProvider.System));

            return;
        }

        if (string.IsNullOrWhiteSpace(renaper.UrlBase) ||
            string.IsNullOrWhiteSpace(renaper.ApiKey))
        {
            throw new InvalidOperationException(
                "Falta configurar Renaper:UrlBase y " +
                "Renaper:ApiKey. En desarrollo se puede " +
                "usar Renaper:Modo = Stub.");
        }

        services.AddHttpClient<
            IRenaperCliente,
            RenaperClienteHttp>(cliente =>
            {
                cliente.BaseAddress = new Uri(
                    renaper.UrlBase.TrimEnd('/') + "/");

                cliente.Timeout = TimeSpan.FromSeconds(
                    renaper.TimeoutSegundos);

                cliente.DefaultRequestHeaders.Add(
                    RenaperClienteHttp.HeaderApiKey,
                    renaper.ApiKey);
            });
    }
}
    