
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Xunit;

namespace ArcaSim.Tests.Soap;

public sealed class ModelosSoapTests
{
    private static readonly XNamespace Ns = IServiceSoap.Ns;

    // =====================================================
    // MÉTODO AUXILIAR PARA SERIALIZAR OBJETOS A XML
    // =====================================================

    private static XDocument Serializar<T>(T objeto)
    {
        var serializer = new XmlSerializer(typeof(T));

        using var writer = new StringWriter();

        using (var xmlWriter = XmlWriter.Create(
            writer,
            new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8
            }))
        {
            serializer.Serialize(xmlWriter, objeto);
        }

        return XDocument.Parse(writer.ToString());
    }

    // =====================================================
    // PRUEBA 1: AUTENTICACIÓN SOAP
    // =====================================================

    [Fact]
    public void FEAuthRequest_SerializaCorrectamente()
    {
        var auth = new FEAuthRequest
        {
            Token = "API_KEY_PRUEBA",
            Sign = "",
            Cuit = 30712345671
        };

        var xml = Serializar(auth);
        var raiz = Assert.IsType<XElement>(xml.Root);

        // El elemento del XML se llama Auth.
        // El tipo C# continúa siendo FEAuthRequest.
        Assert.Equal(Ns + "Auth", raiz.Name);

        Assert.Equal(
            "API_KEY_PRUEBA",
            (string?)raiz.Element(Ns + "Token"));

        Assert.Equal(
            "",
            (string?)raiz.Element(Ns + "Sign"));

        Assert.Equal(
            "30712345671",
            (string?)raiz.Element(Ns + "Cuit"));

        Assert.Equal(
            new[] { "Token", "Sign", "Cuit" },
            raiz.Elements()
                .Select(x => x.Name.LocalName)
                .ToArray());
    }

    // =====================================================
    // PRUEBA 2: SOLICITUD FECompConsultar
    // =====================================================

    [Fact]
    public void FECompConsultaReq_SerializaCorrectamente()
    {
        var solicitud = new FECompConsultaReq
        {
            CbteTipo = 6,
            CbteNro = 125,
            PtoVta = 1
        };

        var xml = Serializar(solicitud);
        var raiz = Assert.IsType<XElement>(xml.Root);

        Assert.Equal(
            "6",
            (string?)raiz.Element(Ns + "CbteTipo"));

        Assert.Equal(
            "125",
            (string?)raiz.Element(Ns + "CbteNro"));

        Assert.Equal(
            "1",
            (string?)raiz.Element(Ns + "PtoVta"));

        Assert.Equal(
            new[] { "CbteTipo", "CbteNro", "PtoVta" },
            raiz.Elements()
                .Select(x => x.Name.LocalName)
                .ToArray());
    }

    // =====================================================
    // PRUEBA 3: RESPUESTA ÚLTIMO AUTORIZADO
    // =====================================================

    [Fact]
    public void FERecuperaLastCbteResponse_SerializaCorrectamente()
    {
        var respuesta = new FERecuperaLastCbteResponse
        {
            PtoVta = 1,
            CbteTipo = 6,
            CbteNro = 125,
            Errors = new List<SoapError>
            {
                new SoapError
                {
                    Code = 600,
                    Msg = "Clave inválida."
                }
            }
        };

        var xml = Serializar(respuesta);
        var raiz = Assert.IsType<XElement>(xml.Root);

        Assert.Equal(
            "125",
            (string?)raiz.Element(Ns + "CbteNro"));

        var errores = raiz.Element(Ns + "Errors");
        Assert.NotNull(errores);

        var error = errores.Element(Ns + "Err");
        Assert.NotNull(error);

        Assert.Equal(
            "600",
            (string?)error.Element(Ns + "Code"));

        Assert.Equal(
            "Clave inválida.",
            (string?)error.Element(Ns + "Msg"));
    }

    // =====================================================
    // PRUEBA 4: RESPUESTA FECompConsultar
    // =====================================================

    [Fact]
    public void FECompConsultaResponse_SerializaCorrectamente()
    {
        var respuesta = new FECompConsultaResponse
        {
            ResultGet = new FECompConsultaResult
            {
                Resultado = "A",
                CodAutorizacion = "12345678901234",
                EmisionTipo = "CAE",
                FchVto = "20261124",
                FchProceso = "20261114143000",
                PtoVta = 1,
                CbteTipo = 6
            }
        };

        var xml = Serializar(respuesta);
        var raiz = Assert.IsType<XElement>(xml.Root);

        var resultado = raiz.Element(Ns + "ResultGet");
        Assert.NotNull(resultado);

        Assert.Equal(
            "A",
            (string?)resultado.Element(Ns + "Resultado"));

        Assert.Equal(
            "12345678901234",
            (string?)resultado.Element(Ns + "CodAutorizacion"));

        Assert.Equal(
            "20261124",
            (string?)resultado.Element(Ns + "FchVto"));

        Assert.Equal(
            "1",
            (string?)resultado.Element(Ns + "PtoVta"));

        Assert.Equal(
            "6",
            (string?)resultado.Element(Ns + "CbteTipo"));
    }
}
