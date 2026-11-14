using System.Security.Cryptography;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Api.Controllers;

[ApiController]
[Route("api/contribuyentes")]
public class ContribuyentesController(
    ContribuyenteServicio contribuyenteServicio,
    PuntoVentaServicio puntoVentaServicio,
    AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista todos los contribuyentes registrados con sus puntos de venta.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ContribuyenteResponse>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await db.Contribuyentes
            .AsNoTracking()
            .Include(c => c.PuntosVenta)
            .OrderBy(c => c.NombreORazonSocial)
            .ToListAsync(cancellationToken);

        return Ok(lista.Select(MapeadorRest.MapearContribuyente).ToList());
    }

    /// <summary>
    /// Obtiene un contribuyente por su CUIT junto con sus puntos de venta.
    /// </summary>
    [HttpGet("{cuit}")]
    public ActionResult<ContribuyenteResponse> ObtenerPorCuit(string cuit)
    {
        var contribuyente = contribuyenteServicio.ConsultarPorCUIT(cuit.Trim());
        if (contribuyente is null)
            return NotFound(new { mensaje = $"No existe un contribuyente con CUIT {cuit}." });

        return Ok(MapeadorRest.MapearContribuyente(contribuyente));
    }

    /// <summary>
    /// Alta de contribuyente. Si es persona humana, valida identidad y edad mínima con RENAPER.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContribuyenteResponse>> Registrar(
        [FromBody] RegistrarContribuyenteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos del contribuyente son obligatorios." });

        var cuit = request.Cuit?.Trim() ?? string.Empty;
        var condicionIva = (CondicionIva)request.CondicionIva;

        try
        {
            Contribuyente creado;
            bool esHumana = string.Equals(request.TipoPersona, "Humana", StringComparison.OrdinalIgnoreCase);

            if (esHumana)
            {
                creado = await contribuyenteServicio.RegistrarPersonaHumanaAsync(
                    cuit,
                    request.Nombre ?? string.Empty,
                    request.Apellido ?? string.Empty,
                    condicionIva,
                    cancellationToken);
            }
            else
            {
                var juridica = new Contribuyente
                {
                    CUIT = cuit,
                    NombreORazonSocial = request.RazonSocial?.Trim() ?? string.Empty,
                    TipoPersona = TipoPersona.Juridica,
                    CondicionIva = condicionIva
                };
                contribuyenteServicio.Registrar(juridica);
                creado = juridica;
            }

            return CreatedAtAction(
                nameof(ObtenerPorCuit),
                new { cuit = creado.CUIT },
                MapeadorRest.MapearContribuyente(creado));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Según docs/renaper.md: 409 si ya existe, 422 si RENAPER rechazó identidad/edad
            if (ex.Message.Contains("Ya existe", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { mensaje = ex.Message });

            return UnprocessableEntity(new { mensaje = ex.Message });
        }
        catch (RenaperNoDisponibleException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Alta de un punto de venta para el contribuyente indicado.
    /// </summary>
    [HttpPost("{cuit}/puntos-venta")]
    public ActionResult<PuntoVentaResponse> RegistrarPuntoVenta(
        string cuit,
        [FromBody] RegistrarPuntoVentaRequest request)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos del punto de venta son obligatorios." });

        try
        {
            var puntoVenta = new PuntoVenta
            {
                CUITContribuyente = cuit.Trim(),
                Numero = request.Numero,
                Habilitado = request.Habilitado
            };

            puntoVentaServicio.Registrar(puntoVenta);
            return Created(
                $"/api/contribuyentes/{puntoVenta.CUITContribuyente}",
                MapeadorRest.MapearPuntoVenta(puntoVenta));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("No existe", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { mensaje = ex.Message });

            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Genera o regenera la API Key del contribuyente para usar en el web service SOAP WSFEv1.
    /// Se devuelve en texto plano una única vez.
    /// </summary>
    [HttpPost("{cuit}/api-key")]
    public ActionResult<ApiKeyResponse> GenerarApiKey(string cuit)
    {
        var contribuyente = contribuyenteServicio.ConsultarPorCUIT(cuit.Trim());
        if (contribuyente is null)
            return NotFound(new { mensaje = $"No existe un contribuyente con CUIT {cuit}." });

        var bytes = RandomNumberGenerator.GetBytes(24);
        var token = $"arca_{Convert.ToHexString(bytes).ToLowerInvariant()}";

        return Ok(new ApiKeyResponse
        {
            Cuit = contribuyente.CUIT,
            ApiKey = token,
            FechaGeneracion = DateTimeOffset.Now,
            Mensaje = "Guardá esta API Key en un lugar seguro: por seguridad se muestra una sola vez."
        });
    }
}

