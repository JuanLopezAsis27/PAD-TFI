using ArcaSim.Infraestructura.Seguridad;

namespace ArcaSim.Tests.Seguridad;

public sealed class HasheadorClavePbkdf2Tests
{
    private readonly HasheadorClavePbkdf2 _hasheador = new();

    [Fact]
    public void Hashear_NoGuardaLaClaveYVerificaLaCorrecta()
    {
        var hash = _hasheador.Hashear("Factura2026");

        Assert.DoesNotContain("Factura2026", hash);
        Assert.StartsWith("v1$100000$", hash);
        Assert.True(_hasheador.Verificar("Factura2026", hash));
    }

    [Theory]
    [InlineData("factura2026")]
    [InlineData("Factura2027")]
    [InlineData("")]
    public void Verificar_ClaveDistinta_DevuelveFalse(string clave)
    {
        var hash = _hasheador.Hashear("Factura2026");

        Assert.False(_hasheador.Verificar(clave, hash));
    }

    [Fact]
    public void Hashear_MismaClaveDosVeces_DaHashesDistintos()
    {
        // Cada clave lleva su propia salt aleatoria.
        Assert.NotEqual(_hasheador.Hashear("Factura2026"), _hasheador.Hashear("Factura2026"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-plano")]
    [InlineData("v1$abc$AAAA$BBBB")]
    [InlineData("v2$100000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("v1$100000$no-es-base64$no-es-base64")]
    public void Verificar_HashMalformado_DevuelveFalse(string hash)
    {
        Assert.False(_hasheador.Verificar("Factura2026", hash));
    }
}
