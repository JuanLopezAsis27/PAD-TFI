
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.Xml.Linq;
using System.Xml.Serialization;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class CatalogosSoapTests
{
    // =====================================================
    // MÉTODO AUXILIAR PARA SERIALIZAR XML
    // =====================================================

    private static XElement Serializar<T>(T objeto)
    {
        var serializador = new XmlSerializer(typeof(T));

        using var escritor = new StringWriter();

        serializador.Serialize(escritor, objeto);

        return XDocument.Parse(escritor.ToString()).Root!;
    }

    // =====================================================
    // MÉTODO AUXILIAR PARA BUSCAR ELEMENTOS
    // =====================================================

    private static XElement? Buscar(
        XElement elemento,
        string nombre)
    {
        return elemento.DescendantsAndSelf()
            .FirstOrDefault(x => x.Name.LocalName == nombre);
    }

    // =====================================================
    // PRUEBA 1: FECompTotXRequest
    // =====================================================

    [Fact]
    public void FECompTotXRequest_SerializaLimiteCorrectamente()
    {
        var respuesta = new FERegXReqResponse
        {
            RegXReq = 1
        };

        XElement xml = Serializar(respuesta);

        var elemento = Buscar(xml, "RegXReq");

        Assert.NotNull(elemento);
        Assert.Equal("1", elemento.Value);
    }

    // =====================================================
    // PRUEBA 2: FEParamGetTiposDoc
    // =====================================================

    [Fact]
    public void FEParamGetTiposDoc_SerializaDocumentosCorrectamente()
    {
        var respuesta = new FEParamGetTiposDocResponse
        {
            ResultGet = new List<FEDocTipo>
            {
                new()
                {
                    Id = 80,
                    Desc = "CUIT"
                },
                new()
                {
                    Id = 96,
                    Desc = "DNI"
                },
                new()
                {
                    Id = 99,
                    Desc = "SinIdentificar"
                }
            }
        };

        XElement xml = Serializar(respuesta);

        var documentos = xml.Descendants()
            .Where(x => x.Name.LocalName == "DocTipo")
            .ToList();

        Assert.Equal(3, documentos.Count);

        Assert.Equal(
            "80",
            Buscar(documentos[0], "Id")?.Value);

        Assert.Equal(
            "CUIT",
            Buscar(documentos[0], "Desc")?.Value);

        Assert.Equal(
            "99",
            Buscar(documentos[2], "Id")?.Value);
    }

    // =====================================================
    // PRUEBA 3: FEParamGetTiposConcepto
    // =====================================================

    [Fact]
    public void FEParamGetTiposConcepto_SerializaConceptos()
    {
        var respuesta = new FEParamGetTiposConceptoResponse
        {
            ResultGet = new List<FEConceptoTipo>
            {
                new()
                {
                    Id = 1,
                    Desc = "Productos"
                },
                new()
                {
                    Id = 2,
                    Desc = "Servicios"
                },
                new()
                {
                    Id = 3,
                    Desc = "Productos y Servicios"
                }
            }
        };

        XElement xml = Serializar(respuesta);

        var conceptos = xml.Descendants()
            .Where(x => x.Name.LocalName == "ConceptoTipo")
            .ToList();

        Assert.Equal(3, conceptos.Count);

        Assert.Equal(
            "Productos",
            Buscar(conceptos[0], "Desc")?.Value);

        Assert.Equal(
            "Servicios",
            Buscar(conceptos[1], "Desc")?.Value);

        Assert.Equal(
            "Productos y Servicios",
            Buscar(conceptos[2], "Desc")?.Value);
    }

    // =====================================================
    // PRUEBA 4: FEParamGetPtosVenta
    // =====================================================

    [Fact]
    public void FEParamGetPtosVenta_SerializaPuntoHabilitado()
    {
        var respuesta = new FEParamGetPtosVentaResponse
        {
            ResultGet = new List<FEPtoVenta>
            {
                new()
                {
                    Nro = 1,
                    EmisionTipo = "CAE",
                    Bloqueado = "N"
                }
            }
        };

        XElement xml = Serializar(respuesta);

        var puntoVenta = Buscar(xml, "PtoVenta");

        Assert.NotNull(puntoVenta);

        Assert.Equal(
            "1",
            Buscar(puntoVenta, "Nro")?.Value);

        Assert.Equal(
            "CAE",
            Buscar(puntoVenta, "EmisionTipo")?.Value);

        Assert.Equal(
            "N",
            Buscar(puntoVenta, "Bloqueado")?.Value);
    }

    // =====================================================
    // PRUEBA 5: FEParamGetTiposMonedas
    // =====================================================

    [Fact]
    public void FEParamGetTiposMonedas_SerializaPesos()
    {
        var respuesta = new FEParamGetTiposMonedasResponse
        {
            ResultGet = new List<FEMoneda>
            {
                new()
                {
                    Id = "PES",
                    Desc = "Pesos Argentinos"
                }
            }
        };

        XElement xml = Serializar(respuesta);

        var moneda = Buscar(xml, "Moneda");

        Assert.NotNull(moneda);

        Assert.Equal(
            "PES",
            Buscar(moneda, "Id")?.Value);

        Assert.Equal(
            "Pesos Argentinos",
            Buscar(moneda, "Desc")?.Value);
    }

    // =====================================================
    // PRUEBA 6: NO PUBLICAR RESULTGET SI HAY ERROR
    // =====================================================

    [Fact]
    public void FEParamGetTiposMonedas_ConError_NoSerializaCatalogo()
    {
        var respuesta = new FEParamGetTiposMonedasResponse
        {
            Errors = new List<SoapError>
            {
                new()
                {
                    Code = 600,
                    Msg = "API key inválida."
                }
            }
        };

        XElement xml = Serializar(respuesta);

        Assert.Null(Buscar(xml, "ResultGet"));

        Assert.Equal(
            "600",
            Buscar(xml, "Code")?.Value);

        Assert.Equal(
            "API key inválida.",
            Buscar(xml, "Msg")?.Value);
    }

    // =====================================================
    // PRUEBA 7: NO PUBLICAR FECHAS DESCONOCIDAS
    // =====================================================

    [Fact]
    public void FEParamGetTiposDoc_SinVigencias_OmiteFechas()
    {
        var respuesta = new FEParamGetTiposDocResponse
        {
            ResultGet = new List<FEDocTipo>
            {
                new()
                {
                    Id = 96,
                    Desc = "DNI",
                    FchDesde = null,
                    FchHasta = null
                }
            }
        };

        XElement xml = Serializar(respuesta);

        Assert.Null(Buscar(xml, "FchDesde"));
        Assert.Null(Buscar(xml, "FchHasta"));
    }

    // =====================================================
    // PRUEBA 8: ERROR EN FECompTotXRequest
    // =====================================================

    [Fact]
    public void FECompTotXRequest_ConError_SerializaCodigo600()
    {
        var respuesta = new FERegXReqResponse
        {
            Errors = new List<SoapError>
            {
                new()
                {
                    Code = 600,
                    Msg = "La API key es inválida."
                }
            }
        };

        XElement xml = Serializar(respuesta);

        Assert.Equal(
            "600",
            Buscar(xml, "Code")?.Value);

        Assert.Equal(
            "La API key es inválida.",
            Buscar(xml, "Msg")?.Value);
    }
}
