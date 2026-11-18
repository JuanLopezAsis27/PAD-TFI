
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Persistencia;
using ArcaSim.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace ArcaSim.Tests.Seguridad;

public sealed class AdministradorApiKeysPostgresTests
{
    private readonly ITestOutputHelper _output;

    public AdministradorApiKeysPostgresTests(
        ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Categoria", "IntegracionPostgres")]
    public async Task GenerarValidarYRevocarApiKey_ConPostgres()
    {
        // =================================================
        // 1. OBTENER CONEXIÓN DE PRUEBAS
        // =================================================

        string? connectionString = Environment.GetEnvironmentVariable(
            "ARCASIM_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _output.WriteLine(
                "Prueba no ejecutada: falta configurar " +
                "ARCASIM_TEST_CONNECTION_STRING.");

            return;
        }

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var contexto = new AppDbContext(opciones);

        // =================================================
        // 2. BUSCAR UN CONTRIBUYENTE EXISTENTE
        // =================================================

        string? cuit = await contexto.Contribuyentes
            .AsNoTracking()
            .Select(x => x.CUIT)
            .FirstOrDefaultAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(cuit),
            "La base de pruebas debe tener al menos " +
            "un contribuyente registrado.");

        // =================================================
        // 3. CREAR SERVICIOS REALES
        // =================================================

        IAdministradorApiKeysSoap administrador =
            new AdministradorApiKeysSoap(contexto);

        IConsultaApiKeys consulta =
            new ConsultaApiKeysRepositorio(contexto);

        IValidadorApiKey validador =
            new ValidadorApiKey(consulta);

        int? idCreado = null;

        try
        {
            // =============================================
            // 4. GENERAR UNA CLAVE
            // =============================================

            var generada = await administrador.GenerarAsync(cuit!);

            idCreado = generada.Id;

            Assert.True(generada.Id > 0);
            Assert.Equal(cuit, generada.Cuit);
            Assert.Matches("^[0-9A-F]{64}$", generada.Clave);

            string hash = GeneradorApiKey.CalcularHash(
                generada.Clave);

            // =============================================
            // 5. COMPROBAR SU REGISTRO EN POSTGRESQL
            // =============================================

            var almacenada = await contexto.ApiKeysSoap
                .AsNoTracking()
                .SingleAsync(x => x.Id == generada.Id);

            Assert.Equal(hash, almacenada.Hash);
            Assert.Equal(cuit, almacenada.Cuit);
            Assert.False(almacenada.Revocada);

            // La clave original no debe almacenarse.
            Assert.NotEqual(generada.Clave, almacenada.Hash);

            // =============================================
            // 6. VALIDAR LA CLAVE ACTIVA
            // =============================================

            var resultadoActivo = validador.Validar(
                generada.Clave,
                cuit!);

            Assert.True(
                resultadoActivo.EsValida,
                resultadoActivo.Mensaje);

            // =============================================
            // 7. REVOCAR LA CLAVE
            // =============================================

            bool revocada = await administrador.RevocarAsync(
                generada.Id,
                cuit!);

            Assert.True(revocada);

            var registroRevocado = await contexto.ApiKeysSoap
                .AsNoTracking()
                .SingleAsync(x => x.Id == generada.Id);

            Assert.True(registroRevocado.Revocada);
            Assert.NotNull(registroRevocado.FechaRevocacion);

            // =============================================
            // 8. RECHAZAR LA CLAVE REVOCADA
            // =============================================

            var resultadoRevocado = validador.Validar(
                generada.Clave,
                cuit!);

            Assert.False(resultadoRevocado.EsValida);
            Assert.Equal(600, resultadoRevocado.CodigoError);

            // =============================================
            // 9. NO REVOCAR DOS VECES
            // =============================================

            bool segundaRevocacion =
                await administrador.RevocarAsync(
                    generada.Id,
                    cuit!);

            Assert.False(segundaRevocacion);

            _output.WriteLine(
                "Generación, almacenamiento, validación " +
                "y revocación correctas.");
        }
        finally
        {
            // =============================================
            // 10. LIMPIAR SOLO LA CLAVE TEMPORAL
            // =============================================

            if (idCreado.HasValue)
            {
                await contexto.ApiKeysSoap
                    .Where(x => x.Id == idCreado.Value &&
                                x.Cuit == cuit)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    [Trait("Categoria", "IntegracionPostgres")]
    public async Task RegenerarApiKey_RevocaLaAnteriorYLaNuevaFunciona_ConPostgres()
    {
        string? connectionString = Environment.GetEnvironmentVariable(
            "ARCASIM_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _output.WriteLine(
                "Prueba no ejecutada: falta configurar " +
                "ARCASIM_TEST_CONNECTION_STRING.");

            return;
        }

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var contexto = new AppDbContext(opciones);

        string? cuit = await contexto.Contribuyentes
            .AsNoTracking()
            .Select(x => x.CUIT)
            .FirstOrDefaultAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(cuit),
            "La base de pruebas debe tener al menos " +
            "un contribuyente registrado.");

        IAdministradorApiKeysSoap administrador =
            new AdministradorApiKeysSoap(contexto);

        IValidadorApiKey validador =
            new ValidadorApiKey(new ConsultaApiKeysRepositorio(contexto));

        // Regenerar revoca todas las claves activas de la CUIT: se anotan las
        // que ya existían para dejarlas como estaban al terminar.
        List<int> activasPrevias = await contexto.ApiKeysSoap
            .Where(x => x.Cuit == cuit && !x.Revocada)
            .Select(x => x.Id)
            .ToListAsync();

        var creadas = new List<int>();

        try
        {
            var anterior = await administrador.GenerarAsync(cuit!);
            creadas.Add(anterior.Id);

            var (nueva, revocadas) = await administrador.RegenerarAsync(cuit!);
            creadas.Add(nueva.Id);

            Assert.Equal(activasPrevias.Count + 1, revocadas);
            Assert.Matches("^[0-9A-F]{64}$", nueva.Clave);

            var resultadoAnterior = validador.Validar(anterior.Clave, cuit!);
            Assert.False(resultadoAnterior.EsValida);
            Assert.Equal(600, resultadoAnterior.CodigoError);

            var resultadoNueva = validador.Validar(nueva.Clave, cuit!);
            Assert.True(resultadoNueva.EsValida, resultadoNueva.Mensaje);
        }
        finally
        {
            await contexto.ApiKeysSoap
                .Where(x => creadas.Contains(x.Id))
                .ExecuteDeleteAsync();

            await contexto.ApiKeysSoap
                .Where(x => activasPrevias.Contains(x.Id))
                .ExecuteUpdateAsync(a => a
                    .SetProperty(x => x.Revocada, false)
                    .SetProperty(x => x.FechaRevocacion, (DateTime?)null));
        }
    }

    [Fact]
    [Trait("Categoria", "IntegracionPostgres")]
    public async Task RegenerarApiKey_ContribuyenteInexistente_LanzaExcepcion_ConPostgres()
    {
        string? connectionString = Environment.GetEnvironmentVariable(
            "ARCASIM_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _output.WriteLine(
                "Prueba no ejecutada: falta configurar " +
                "ARCASIM_TEST_CONNECTION_STRING.");

            return;
        }

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var contexto = new AppDbContext(opciones);

        IAdministradorApiKeysSoap administrador =
            new AdministradorApiKeysSoap(contexto);

        // CUIT válida que no está dada de alta: la transacción se descarta.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => administrador.RegenerarAsync("20000000001"));
    }
}
