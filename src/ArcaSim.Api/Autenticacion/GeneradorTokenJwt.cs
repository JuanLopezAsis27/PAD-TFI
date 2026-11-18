using System.Security.Claims;
using System.Text;
using ArcaSim.Dominio.Entidades;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ArcaSim.Api.Autenticacion;

/// <summary>Arma el JWT de una sesión de Clave Fiscal. El claim <c>sub</c> es la CUIT/CUIL del usuario.</summary>
public sealed class GeneradorTokenJwt(JwtOpciones opciones, TimeProvider reloj)
{
    public (string Token, DateTimeOffset Expira) Generar(Usuario usuario)
    {
        // Los JWT guardan las fechas en segundos: se redondea para que el portal vea el mismo vencimiento.
        var ahora = DateTimeOffset.FromUnixTimeSeconds(reloj.GetUtcNow().ToUnixTimeSeconds());
        var expira = ahora.AddMinutes(opciones.MinutosDeVigencia);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = opciones.Emisor,
            Audience = opciones.Audiencia,
            IssuedAt = ahora.UtcDateTime,
            NotBefore = ahora.UtcDateTime,
            Expires = expira.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Cuit),
                new Claim(JwtRegisteredClaimNames.GivenName, usuario.Nombre),
                new Claim(JwtRegisteredClaimNames.FamilyName, usuario.Apellido),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            ]),
            SigningCredentials = new SigningCredentials(CrearClaveDeFirma(opciones.Clave), SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }

    public static SymmetricSecurityKey CrearClaveDeFirma(string clave) => new(Encoding.UTF8.GetBytes(clave));
}
