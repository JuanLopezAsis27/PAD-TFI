using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Aplicacion;

public class OpcionesQrTests
{
    [Theory]
    [InlineData("https://arcasim.example.com", "https://arcasim.example.com/fe/qr/")]
    [InlineData("https://arcasim.example.com/", "https://arcasim.example.com/fe/qr/")]
    [InlineData(" http://localhost:5247 ", "http://localhost:5247/fe/qr/")]
    [InlineData("https://ejemplo.com/arcasim/", "https://ejemplo.com/arcasim/fe/qr/")]
    public void ParaPortal_AgregaLaRutaDeLaConstatacion(string urlPortal, string esperada)
    {
        var opciones = OpcionesQr.ParaPortal(urlPortal);

        Assert.Equal(esperada, opciones.UrlBase);
        Assert.False(opciones.ApuntaAArca);
    }

    [Theory]
    [InlineData("localhost:5247")]
    [InlineData("/portal")]
    [InlineData("ftp://arcasim.example.com")]
    [InlineData("https://arcasim.example.com/?a=1")]
    public void ParaPortal_DireccionInvalida_Lanza(string urlPortal)
    {
        Assert.Throws<ArgumentException>(() => OpcionesQr.ParaPortal(urlPortal));
    }

    [Fact]
    public void Arca_ApuntaALaPaginaDeArca()
    {
        Assert.Equal(GeneradorQrComprobante.UrlBaseArca, OpcionesQr.Arca.UrlBase);
        Assert.True(OpcionesQr.Arca.ApuntaAArca);
    }
}
