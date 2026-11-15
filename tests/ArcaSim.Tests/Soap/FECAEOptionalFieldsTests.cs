
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.Xml.Linq;
using System.Xml.Serialization;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class FECAEOptionalFieldsTests
{
    private static readonly XNamespace Ns = IServiceSoap.Ns;

    private static XElement Serializar(FECAEDetRequest detalle)
    {
        var root = new XmlRootAttribute("FECAEDetRequest")
        {
            Namespace = IServiceSoap.Ns
        };

        var serializer = new XmlSerializer(
            typeof(FECAEDetRequest), root);

        using var writer = new StringWriter();
        serializer.Serialize(writer, detalle);

        return XDocument.Parse(writer.ToString()).Root!;
    }

    [Fact]
    public void CamposOpcionalesNulos_NoSeSerializan()
    {
        var detalle = new FECAEDetRequest();

        var xml = Serializar(detalle);

        Assert.Null(xml.Element(Ns + "FchServDesde"));
        Assert.Null(xml.Element(Ns + "FchServHasta"));
        Assert.Null(xml.Element(Ns + "FchVtoPago"));
        Assert.Null(xml.Element(Ns + "CanMisMonExt"));
        Assert.Null(xml.Element(Ns + "CondicionIVAReceptorId"));
        Assert.Null(xml.Element(Ns + "Iva"));
        Assert.Null(xml.Element(Ns + "Tributos"));
        Assert.Null(xml.Element(Ns + "CbtesAsoc"));
    }

    [Fact]
    public void CamposOpcionalesInformados_SeSerializan()
    {
        var detalle = new FECAEDetRequest
        {
            FchServDesde = "20261101",
            FchServHasta = "20261130",
            FchVtoPago = "20261210",
            CondicionIVAReceptorId = 1,
            CanMisMonExt = "N",
            Iva = new List<FEAlicIva>
            {
                new()
                {
                    Id = 5,
                    BaseImp = 10000,
                    Importe = 2100
                }
            }
        };

        var xml = Serializar(detalle);

        Assert.Equal("20261101",
            (string?)xml.Element(Ns + "FchServDesde"));

        Assert.Equal("20261130",
            (string?)xml.Element(Ns + "FchServHasta"));

        Assert.Equal("20261210",
            (string?)xml.Element(Ns + "FchVtoPago"));

        Assert.Equal("1",
            (string?)xml.Element(Ns + "CondicionIVAReceptorId"));

        Assert.Equal("N",
            (string?)xml.Element(Ns + "CanMisMonExt"));

        Assert.NotNull(xml.Element(Ns + "Iva")?
            .Element(Ns + "AlicIva"));
    }

    [Fact]
    public void ListasVacias_NoSeSerializan()
    {
        var detalle = new FECAEDetRequest
        {
            Iva = new(),
            Tributos = new(),
            CbtesAsoc = new(),
            Opcionales = new()
        };

        var xml = Serializar(detalle);

        Assert.Null(xml.Element(Ns + "Iva"));
        Assert.Null(xml.Element(Ns + "Tributos"));
        Assert.Null(xml.Element(Ns + "CbtesAsoc"));
        Assert.Null(xml.Element(Ns + "Opcionales"));
    }
}
