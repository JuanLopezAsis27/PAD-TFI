
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap
{
    // =====================================================
    // FEParamGetTiposConcepto
    // =====================================================

    public FEParamGetTiposConceptoResponse FEParamGetTiposConcepto(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetTiposConceptoResponse();

        // Validar API key y CUIT.
        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        // Recuperar conceptos disponibles en el dominio.
        respuesta.ResultGet = Enum
            .GetValues<ConceptoComprobante>()
            .Select(concepto => new FEConceptoTipo
            {
                Id = (int)concepto,
                Desc = ObtenerDescripcionConcepto(concepto)
            })
            .OrderBy(item => item.Id)
            .ToList();

        return respuesta;
    }

    // =====================================================
    // DESCRIPCIONES DE LOS CONCEPTOS
    // =====================================================

    private static string ObtenerDescripcionConcepto(
        ConceptoComprobante concepto)
    {
        return (int)concepto switch
        {
            1 => "Productos",
            2 => "Servicios",
            3 => "Productos y Servicios",
            _ => concepto.ToString()
        };
    }
}
