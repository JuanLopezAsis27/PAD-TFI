
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class FECAESolicitarTests
{
    private static readonly XNamespace Ns = IServiceSoap.Ns;

    // =====================================================
    // SERIALIZACIÓN XML
    // =====================================================

    private static XDocument Serializar<T>(
        T objeto,
        string nombreRaiz)
    {
        var raiz = new XmlRootAttribute(nombreRaiz)
        {
            Namespace = IServiceSoap.Ns
        };

        var serializer = new XmlSerializer(typeof(T), raiz);

        using var writer = new StringWriter();

        serializer.Serialize(writer, objeto);

        return XDocument.Parse(writer.ToString());
    }

    // =====================================================
    // PRUEBA 1: CABECERA DE SOLICITUD
    // =====================================================

    [Fact]
    public void FECAERequest_SerializaCabeceraCorrectamente()
    {
        var solicitud = new FECAERequest
        {
            FeCabReq = new FECAECabRequest
            {
                CantReg = 1,
                PtoVta = 1,
                CbteTipo = 6
            }
        };

        var xml = Serializar(solicitud, "FeCAEReq");

        var cabecera = xml.Root?.Element(Ns + "FeCabReq");

        Assert.NotNull(cabecera);

        Assert.Equal(
            "1",
            (string?)cabecera.Element(Ns + "CantReg"));

        Assert.Equal(
            "1",
            (string?)cabecera.Element(Ns + "PtoVta"));

        Assert.Equal(
            "6",
            (string?)cabecera.Element(Ns + "CbteTipo"));

        Assert.Equal(
            new[] { "CantReg", "PtoVta", "CbteTipo" },
            cabecera.Elements()
                .Select(x => x.Name.LocalName)
                .ToArray());
    }

    // =====================================================
    // PRUEBA 2: DETALLE DE LA FACTURA
    // =====================================================

    [Fact]
    public void FECAERequest_SerializaDetalleCorrectamente()
    {
        var solicitud = new FECAERequest
        {
            FeCabReq = new FECAECabRequest
            {
                CantReg = 1,
                PtoVta = 1,
                CbteTipo = 6
            },

            FeDetReq = new List<FECAEDetRequest>
            {
                new FECAEDetRequest
                {
                    Concepto = 1,
                    DocTipo = 96,
                    DocNro = 12345678,
                    CbteDesde = 126,
                    CbteHasta = 126,
                    CbteFch = "20261114",
                    ImpTotal = 12100,
                    ImpTotConc = 0,
                    ImpNeto = 10000,
                    ImpOpEx = 0,
                    ImpTrib = 0,
                    ImpIVA = 2100,
                    MonId = "PES",
                    MonCotiz = 1
                }
            }
        };

        var xml = Serializar(solicitud, "FeCAEReq");

        var detalles = xml.Root?.Element(Ns + "FeDetReq");

        Assert.NotNull(detalles);

        var detalle = detalles.Element(Ns + "FECAEDetRequest");

        Assert.NotNull(detalle);

        Assert.Equal(
            "1",
            (string?)detalle.Element(Ns + "Concepto"));

        Assert.Equal(
            "96",
            (string?)detalle.Element(Ns + "DocTipo"));

        Assert.Equal(
            "12345678",
            (string?)detalle.Element(Ns + "DocNro"));

        Assert.Equal(
            "126",
            (string?)detalle.Element(Ns + "CbteDesde"));

        Assert.Equal(
            "126",
            (string?)detalle.Element(Ns + "CbteHasta"));

        Assert.Equal(
            "20261114",
            (string?)detalle.Element(Ns + "CbteFch"));

        Assert.Equal(
            "12100",
            (string?)detalle.Element(Ns + "ImpTotal"));

        Assert.Equal(
            "10000",
            (string?)detalle.Element(Ns + "ImpNeto"));

        Assert.Equal(
            "2100",
            (string?)detalle.Element(Ns + "ImpIVA"));

        Assert.Equal(
            "PES",
            (string?)detalle.Element(Ns + "MonId"));
    }

    // =====================================================
    // PRUEBA 3: ALÍCUOTAS DE IVA
    // =====================================================

    [Fact]
    public void FECAERequest_SerializaIvaCorrectamente()
    {
        var solicitud = new FECAERequest
        {
            FeCabReq = new FECAECabRequest
            {
                CantReg = 1,
                PtoVta = 1,
                CbteTipo = 6
            },

            FeDetReq = new List<FECAEDetRequest>
            {
                new FECAEDetRequest
                {
                    Concepto = 1,
                    DocTipo = 96,
                    DocNro = 12345678,
                    CbteDesde = 126,
                    CbteHasta = 126,
                    CbteFch = "20261114",

                    Iva = new List<FEAlicIva>
                    {
                        new FEAlicIva
                        {
                            Id = 5,
                            BaseImp = 10000,
                            Importe = 2100
                        }
                    }
                }
            }
        };

        var xml = Serializar(solicitud, "FeCAEReq");

        var detalle = xml.Root?
            .Element(Ns + "FeDetReq")?
            .Element(Ns + "FECAEDetRequest");

        Assert.NotNull(detalle);

        var iva = detalle.Element(Ns + "Iva");

        Assert.NotNull(iva);

        var alicuota = iva.Element(Ns + "AlicIva");

        Assert.NotNull(alicuota);

        Assert.Equal(
            "5",
            (string?)alicuota.Element(Ns + "Id"));

        Assert.Equal(
            "10000",
            (string?)alicuota.Element(Ns + "BaseImp"));

        Assert.Equal(
            "2100",
            (string?)alicuota.Element(Ns + "Importe"));
    }

    // =====================================================
    // PRUEBA 4: RESPUESTA CON CAE
    // =====================================================

    [Fact]
    public void FECAEResponse_SerializaAutorizacionCorrectamente()
    {
        var respuesta = new FECAEResponse
        {
            FeCabResp = new FECAECabResponse
            {
                Cuit = 30712345671,
                PtoVta = 1,
                CbteTipo = 6,
                FchProceso = "20261114143000",
                CantReg = 1,
                Resultado = "A",
                Reproceso = "N"
            },

            FeDetResp = new List<FECAEDetResponse>
            {
                new FECAEDetResponse
                {
                    Concepto = 1,
                    DocTipo = 96,
                    DocNro = 12345678,
                    CbteDesde = 126,
                    CbteHasta = 126,
                    CbteFch = "20261114",
                    Resultado = "A",
                    CAE = "12345678901234",
                    CAEFchVto = "20261124"
                }
            }
        };

        var xml = Serializar(
            respuesta,
            "FECAESolicitarResult");

        var cabecera = xml.Root?.Element(Ns + "FeCabResp");

        Assert.NotNull(cabecera);

        Assert.Equal(
            "A",
            (string?)cabecera.Element(Ns + "Resultado"));

        Assert.Equal(
            "30712345671",
            (string?)cabecera.Element(Ns + "Cuit"));

        var detalle = xml.Root?
            .Element(Ns + "FeDetResp")?
            .Element(Ns + "FECAEDetResponse");

        Assert.NotNull(detalle);

        Assert.Equal(
            "A",
            (string?)detalle.Element(Ns + "Resultado"));

        Assert.Equal(
            "12345678901234",
            (string?)detalle.Element(Ns + "CAE"));

        Assert.Equal(
            "20261124",
            (string?)detalle.Element(Ns + "CAEFchVto"));
    }
}
