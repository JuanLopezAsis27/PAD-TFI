
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.ServiceModel;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial interface IServiceSoap
{
    [OperationContract(
        Action = Ns + "FEParamGetTiposMonedas")]
    FEParamGetTiposMonedasResponse FEParamGetTiposMonedas(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth);
}
