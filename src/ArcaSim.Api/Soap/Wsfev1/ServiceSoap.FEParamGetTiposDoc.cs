
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap
{
    // =====================================================
    // FEParamGetTiposDoc
    // =====================================================

    public FEParamGetTiposDocResponse FEParamGetTiposDoc(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetTiposDocResponse();

        // ================================================
        // VALIDACIÓN DE AUTENTICACIÓN
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
        // RECUPERAR TIPOS DE DOCUMENTO DEL DOMINIO
        // ================================================

        respuesta.ResultGet = Enum
            .GetValues<TipoDocumento>()
            .Select(tipo => new FEDocTipo
            {
                Id = (int)tipo,
                Desc = tipo.ToString()
            })
            .OrderBy(item => item.Id)
            .ToList();

        return respuesta;
    }
}
