using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArcaSim.Api.Autenticacion;

public static class ExtensionesSesion
{
    /// <summary>CUIT/CUIL de la cuenta que hizo el pedido (claim <c>sub</c> del JWT).</summary>
    public static string ObtenerCuit(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("La sesión no tiene la CUIT/CUIL del usuario.");
    }

    /// <summary>403: la cuenta no representa al contribuyente sobre el que quiere operar.</summary>
    public static ObjectResult NoRepresenta(this ControllerBase controller, string cuit)
    {
        return controller.StatusCode(StatusCodes.Status403Forbidden,
            new { mensaje = $"Tu cuenta no representa al CUIT {cuit}." });
    }
}
