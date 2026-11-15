
using ArcaSim.Api.Soap.Wsfev1.Models;
using System.ServiceModel;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial interface IServiceSoap
{
    [OperationContract(
        Action = Ns + "FECompTotXRequest")]
    FERegXReqResponse FECompTotXRequest(
        [MessageParameter(Name = "Auth")]
        FEAuthRequest Auth);
}
