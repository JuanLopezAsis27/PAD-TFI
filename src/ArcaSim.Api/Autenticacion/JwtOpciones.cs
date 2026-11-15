using System.Text;

namespace ArcaSim.Api.Autenticacion;

/// <summary>
/// Configuración de la sección "Jwt" de appsettings: el token que la API entrega al ingresar con Clave
/// Fiscal y que el portal manda en cada llamada a la API REST.
/// </summary>
public sealed class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Emisor { get; set; } = "ArcaSim.Api";
    public string Audiencia { get; set; } = "ArcaSim.Web";

    /// <summary>
    /// Secreto para firmar los tokens (HS256), de al menos 32 caracteres. En producción se configura con
    /// la variable de entorno Jwt__Clave, nunca en git.
    /// </summary>
    public string Clave { get; set; } = "";

    /// <summary>Duración de la sesión. El portal vence la cookie al mismo tiempo.</summary>
    public int MinutosDeVigencia { get; set; } = 120;

    /// <summary>
    /// Controla la configuración al arrancar, para no descubrir el problema recién en el primer ingreso.
    /// </summary>
    public void Validar()
    {
        if (Encoding.UTF8.GetByteCount(Clave) < 32)
        {
            throw new InvalidOperationException(
                "Falta configurar Jwt:Clave (al menos 32 caracteres). En producción se pasa con la variable " +
                "de entorno Jwt__Clave.");
        }

        if (MinutosDeVigencia <= 0)
            throw new InvalidOperationException("Jwt:MinutosDeVigencia tiene que ser mayor que cero.");
    }
}
