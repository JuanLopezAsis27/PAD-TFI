
using ArcaSim.Api.Soap.Wsfev1.Models;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap
{
    // =====================================================
    // FECompTotXRequest
    // =====================================================

    public FERegXReqResponse FECompTotXRequest(
        FEAuthRequest Auth)
    {
        var respuesta = new FERegXReqResponse();

        // Validar la API key y la CUIT.
        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        // Actualmente FECAESolicitar admite
        // un comprobante por solicitud.
        respuesta.RegXReq = 1;

        return respuesta;
    }
}
