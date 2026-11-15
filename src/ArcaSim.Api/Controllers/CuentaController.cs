using ArcaSim.Api.Autenticacion;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcaSim.Api.Controllers;

/// <summary>
/// Clave Fiscal: obtenerla (registro con validación de identidad en RENAPER) e ingresar con ella.
/// </summary>
[ApiController]
[Route("api/cuenta")]
public class CuentaController(ClaveFiscalServicio claveFiscal, GeneradorTokenJwt generadorToken) : ControllerBase
{
    /// <summary>
    /// Obtener la Clave Fiscal. Valida con RENAPER que el nombre y el apellido correspondan al DNI de la
    /// CUIT/CUIL y que la persona tenga al menos 18 años. Consume un crédito de RENAPER por pedido válido.
    /// </summary>
    [HttpPost("registro")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioResponse>> Registrar([FromBody] RegistrarCuentaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos de la cuenta son obligatorios." });

        try
        {
            var usuario = await claveFiscal.RegistrarAsync(request.Cuit, request.Nombre, request.Apellido,
                request.Email, request.Clave, cancellationToken);

            return Created("/api/cuenta", MapeadorRest.MapearUsuario(usuario, []));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (IdentidadNoValidadaException ex)
        {
            return UnprocessableEntity(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (RenaperNoDisponibleException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Ingreso con CUIT/CUIL y clave. Devuelve el token para el header <c>Authorization: Bearer</c>.
    /// Después de 5 claves incorrectas seguidas, la clave queda bloqueada 15 minutos (423).
    /// </summary>
    [HttpPost("login")]
    public ActionResult<SesionResponse> IniciarSesion([FromBody] IniciarSesionRequest request)
    {
        if (request is null)
            return BadRequest(new { mensaje = "La CUIT/CUIL y la clave son obligatorias." });

        var resultado = claveFiscal.IniciarSesion(request.Cuit, request.Clave);

        switch (resultado.Estado)
        {
            case EstadoInicioSesion.Exitoso:
                var usuario = resultado.Usuario!;
                var (token, expira) = generadorToken.Generar(usuario);

                return Ok(new SesionResponse
                {
                    Token = token,
                    Expira = expira,
                    Usuario = MapeadorRest.MapearUsuario(usuario, claveFiscal.ObtenerCuitsRepresentados(usuario.Cuit))
                });

            case EstadoInicioSesion.Bloqueado:
                var hasta = DateTime.SpecifyKind(resultado.BloqueadoHasta!.Value, DateTimeKind.Utc);

                return StatusCode(StatusCodes.Status423Locked, new
                {
                    mensaje = $"Tu clave quedó bloqueada por {ClaveFiscalServicio.IntentosAntesDeBloquear} intentos " +
                        $"fallidos. Podés volver a intentar a las {hasta.ToLocalTime():HH:mm}.",
                    bloqueadoHasta = hasta
                });

            default:
                return Unauthorized(new { mensaje = "CUIT/CUIL o clave incorrectos." });
        }
    }

    /// <summary>Datos de la cuenta con la que se ingresó.</summary>
    [Authorize]
    [HttpGet]
    public ActionResult<UsuarioResponse> Obtener()
    {
        var cuit = User.ObtenerCuit();
        var usuario = claveFiscal.ObtenerUsuario(cuit);

        if (usuario is null)
            return Unauthorized(new { mensaje = "La cuenta de la sesión ya no existe." });

        return Ok(MapeadorRest.MapearUsuario(usuario, claveFiscal.ObtenerCuitsRepresentados(cuit)));
    }
}
