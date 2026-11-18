using System.Security.Claims;
using ArcaSim.Contratos.Responses;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ArcaSim.Web.Servicios;

/// <summary>Datos que el portal guarda en la cookie de sesión al ingresar con Clave Fiscal.</summary>
public static class ClaimsSesion
{
    /// <summary>JWT que entregó la API; se manda en cada llamada REST.</summary>
    public const string Token = "arcasim:token";

    public static ClaimsPrincipal Crear(SesionResponse sesion)
    {
        var u = sesion.Usuario;
        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, u.Cuit),
            new Claim(ClaimTypes.Name, $"{u.Apellido}, {u.Nombre}"),
            new Claim(ClaimTypes.GivenName, u.Nombre),
            new Claim(ClaimTypes.Surname, u.Apellido),
            new Claim(Token, sesion.Token)
        ], CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identidad);
    }

    /// <summary>CUIT/CUIL de la sesión, o null si no hay sesión.</summary>
    public static string? ObtenerCuit(this ClaimsPrincipal usuario) =>
        usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
