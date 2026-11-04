using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

// Los valores de los enums se usan como IDs en el web service, así que deben coincidir con las
// tablas de ARCA.
public class TablasArcaTests
{
    [Theory]
    [InlineData(CondicionIva.ResponsableInscripto, 1)]
    [InlineData(CondicionIva.Exento, 4)]
    [InlineData(CondicionIva.ConsumidorFinal, 5)]
    [InlineData(CondicionIva.Monotributista, 6)]
    [InlineData(CondicionIva.SujetoNoCategorizado, 7)]
    [InlineData(CondicionIva.ProveedorDelExterior, 8)]
    [InlineData(CondicionIva.ClienteDelExterior, 9)]
    [InlineData(CondicionIva.IvaLiberadoLey19640, 10)]
    [InlineData(CondicionIva.MonotributistaSocial, 13)]
    [InlineData(CondicionIva.IvaNoAlcanzado, 15)]
    [InlineData(CondicionIva.MonotributoTrabajadorIndependientePromovido, 16)]
    public void CondicionIva_TieneElIdDeArca(CondicionIva condicion, int idArca)
    {
        Assert.Equal(idArca, (int)condicion);
    }

    [Theory]
    [InlineData(TipoDocumento.CUIT, 80)]
    [InlineData(TipoDocumento.CUIL, 86)]
    [InlineData(TipoDocumento.DNI, 96)]
    [InlineData(TipoDocumento.SinIdentificar, 99)]
    public void TipoDocumento_TieneElIdDeArca(TipoDocumento tipo, int idArca)
    {
        Assert.Equal(idArca, (int)tipo);
    }

    [Theory]
    [InlineData("PES")]
    [InlineData("DOL")]
    [InlineData("060")]
    [InlineData("012")]
    [InlineData("033")]
    [InlineData("011")]
    public void ValidadorMoneda_CodigoDeArca_EsValido(string moneda)
    {
        Assert.True(ValidadorMoneda.EsValida(moneda));
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("BRL")]
    [InlineData("USD")]
    public void ValidadorMoneda_CodigoIso_NoEsValido(string moneda)
    {
        Assert.False(ValidadorMoneda.EsValida(moneda));
    }
}
