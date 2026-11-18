using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorClaveFiscalTests
{
    private const string Cuit = "20401112228";

    [Theory]
    [InlineData("Factura2026")]
    [InlineData("ab12CDefgh")]           // exactamente 10 caracteres
    [InlineData("Clave-Segura.2026!")]   // admite símbolos
    public void ClaveQueCumpleLosRequisitos_EsValida(string clave)
    {
        Assert.True(ValidadorClaveFiscal.EsValida(clave, Cuit));
        Assert.Empty(ValidadorClaveFiscal.RequisitosIncumplidos(clave, Cuit));
    }

    [Theory]
    [InlineData("Fact2026", "entre 10 y 64")]
    [InlineData("Facturacion", "2 números")]
    [InlineData("Ab1234567890", "4 letras")]
    [InlineData("factura2026", "mayúscula")]
    [InlineData("FACTURA2026", "minúscula")]
    [InlineData("Factura 2026", "espacios")]
    public void ClaveQueNoCumpleUnRequisito_LoInforma(string clave, string requisito)
    {
        var incumplidos = ValidadorClaveFiscal.RequisitosIncumplidos(clave, Cuit);

        Assert.Contains(incumplidos, r => r.Contains(requisito, StringComparison.OrdinalIgnoreCase));
        Assert.False(ValidadorClaveFiscal.EsValida(clave, Cuit));
    }

    [Fact]
    public void ClaveDeMasDe64Caracteres_NoEsValida()
    {
        var clave = "Ab12" + new string('x', 61);

        Assert.Contains(ValidadorClaveFiscal.RequisitosIncumplidos(clave), r => r.Contains("entre 10 y 64"));
    }

    [Theory]
    [InlineData("Lucas40111222")]  // el DNI
    [InlineData("Ab20401112228")]  // la CUIT completa
    public void ClaveConLaCuitOElDni_NoEsValida(string clave)
    {
        Assert.Contains(ValidadorClaveFiscal.RequisitosIncumplidos(clave, Cuit), r => r.Contains("DNI"));
    }

    [Fact]
    public void SinCuit_NoControlaLosDatosPersonales()
    {
        Assert.True(ValidadorClaveFiscal.EsValida("Lucas40111222"));
    }

    [Fact]
    public void ClaveNula_InformaLosRequisitos()
    {
        Assert.NotEmpty(ValidadorClaveFiscal.RequisitosIncumplidos(null, Cuit));
    }
}
