
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Infraestructura.Persistencia;
using ArcaSim.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace ArcaSim.Tests.Seguridad;

public sealed class AdministradorApiKeysSoapTests
{
    // =====================================================
    // CREAR ADMINISTRADOR PARA PRUEBAS
    // =====================================================

    private static (
        IAdministradorApiKeysSoap Administrador,
        AppDbContext Contexto)
        CrearEntorno()
    {
        // Estas pruebas verifican condiciones que se
        // resuelven antes de acceder a PostgreSQL.
        //
        // Por eso no necesitamos configurar una conexión.
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        var contexto = new AppDbContext(opciones);

        IAdministradorApiKeysSoap administrador =
            new AdministradorApiKeysSoap(contexto);

        return (administrador, contexto);
    }

    // =====================================================
    // PRUEBA 1: CUIT INVÁLIDA AL GENERAR
    // =====================================================

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("30712ABC671")]
    public async Task GenerarAsync_CuitInvalida_LanzaExcepcion(
        string cuit)
    {
        var entorno = CrearEntorno();

        using var contexto = entorno.Contexto;

        await Assert.ThrowsAsync<ArgumentException>(
            async () =>
                await entorno.Administrador.GenerarAsync(cuit));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("30712ABC671")]
    public async Task RegenerarAsync_CuitInvalida_LanzaExcepcionSinTocarLaBase(
        string cuit)
    {
        var entorno = CrearEntorno();

        using var contexto = entorno.Contexto;

        // El contexto no tiene base configurada: si llegara a abrir la
        // transacción fallaría con otra excepción.
        await Assert.ThrowsAsync<ArgumentException>(
            async () =>
                await entorno.Administrador.RegenerarAsync(cuit));
    }

    // =====================================================
    // PRUEBA 2: ID INVÁLIDO AL REVOCAR
    // =====================================================

    [Fact]
    public async Task RevocarAsync_IdInvalido_DevuelveFalse()
    {
        var entorno = CrearEntorno();

        using var contexto = entorno.Contexto;

        bool resultado = await entorno.Administrador.RevocarAsync(
            0,
            "30712345671");

        Assert.False(resultado);
    }

    // =====================================================
    // PRUEBA 3: CUIT VACÍA AL REVOCAR
    // =====================================================

    [Fact]
    public async Task RevocarAsync_CuitVacia_DevuelveFalse()
    {
        var entorno = CrearEntorno();

        using var contexto = entorno.Contexto;

        bool resultado = await entorno.Administrador.RevocarAsync(
            1,
            "");

        Assert.False(resultado);
    }

    // =====================================================
    // PRUEBA 4: HASH SHA-256 CORRECTO
    // =====================================================

    [Fact]
    public void CalcularHash_DevuelveSha256Correcto()
    {
        const string clave = "CLAVE_DE_PRUEBA";

        string hash = GeneradorApiKey.CalcularHash(clave);

        byte[] bytes = Encoding.UTF8.GetBytes(clave);
        byte[] hashEsperadoBytes = SHA256.HashData(bytes);
        string hashEsperado = Convert.ToHexString(hashEsperadoBytes);

        Assert.Equal(hashEsperado, hash);
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]{64}$", hash);
    }

    // =====================================================
    // PRUEBA 5: MISMA CLAVE, MISMO HASH
    // =====================================================

    [Fact]
    public void CalcularHash_MismaClave_DevuelveMismoResultado()
    {
        const string clave = "CLAVE_DE_PRUEBA";

        string primerHash =
            GeneradorApiKey.CalcularHash(clave);

        string segundoHash =
            GeneradorApiKey.CalcularHash(clave);

        Assert.Equal(primerHash, segundoHash);
    }

    // =====================================================
    // PRUEBA 6: CLAVES DIFERENTES
    // =====================================================

    [Fact]
    public void CalcularHash_ClavesDistintas_DevuelveHashesDistintos()
    {
        string primerHash =
            GeneradorApiKey.CalcularHash("CLAVE_UNO");

        string segundoHash =
            GeneradorApiKey.CalcularHash("CLAVE_DOS");

        Assert.NotEqual(primerHash, segundoHash);
    }
}
