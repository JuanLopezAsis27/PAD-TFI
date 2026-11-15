
using ArcaSim.Api.Soap.Wsfev1.Models;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap
{
    // =====================================================
    // FEParamGetTiposMonedas
    // =====================================================

    public FEParamGetTiposMonedasResponse FEParamGetTiposMonedas(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetTiposMonedasResponse();

        // ================================================
        // 1. VALIDAR AUTENTICACIÓN
        // ================================================

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        // ================================================
        // 2. CATÁLOGO DE MONEDAS DEL SIMULADOR
        // ================================================

        // Por ahora publicamos solamente PES.
        //
        // No inventamos fechas oficiales de vigencia
        // ni monedas que aún no están verificadas
        // en el circuito de facturación.

        respuesta.ResultGet = new List<FEMoneda>
        {
            new FEMoneda
            {
                Id = "PES",
                Desc = "Pesos Argentinos",
                FchDesde = null,
                FchHasta = null
            }
        };

        return respuesta;
    }
}
