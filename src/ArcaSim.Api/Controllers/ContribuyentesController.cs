using ArcaSim.Api.Autenticacion;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Api.Controllers;

/// <summary>
/// Contribuyentes de la cuenta con la que se ingresó: solo se ven y se operan los que representa.
/// </summary>
[ApiController]
[Authorize]
[Route("api/contribuyentes")]
public class ContribuyentesController(
    ContribuyenteServicio contribuyenteServicio,
    PuntoVentaServicio puntoVentaServicio,
    IAdministradorApiKeysSoap administradorApiKeys,
    AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista los contribuyentes que representa la cuenta, con sus puntos de venta.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ContribuyenteResponse>>> Listar(CancellationToken cancellationToken)
    {
        var cuitUsuario = User.ObtenerCuit();

        var lista = await db.Contribuyentes
            .AsNoTracking()
            .Include(c => c.PuntosVenta)
            .Where(c => db.Representaciones.Any(r => r.CuitUsuario == cuitUsuario && r.CuitRepresentado == c.CUIT))
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
        cuit = cuit.Trim();
        if (!contribuyenteServicio.Representa(User.ObtenerCuit(), cuit))
            return this.NoRepresenta(cuit);

        var contribuyente = contribuyenteServicio.ConsultarPorCUIT(cuit);
        if (contribuyente is null)
            return NotFound(new { mensaje = $"No existe un contribuyente con CUIT {cuit}." });

        return Ok(MapeadorRest.MapearContribuyente(contribuyente));
    }

    /// <summary>
    /// Alta de contribuyente. Persona humana: se inscribe la dueña de la cuenta, con la identidad que validó
    /// RENAPER al obtener la Clave Fiscal. Persona jurídica: la cuenta queda como su representante.
    /// </summary>
    [HttpPost]
    public ActionResult<ContribuyenteResponse> Registrar([FromBody] RegistrarContribuyenteRequest request)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos del contribuyente son obligatorios." });

        var cuitUsuario = User.ObtenerCuit();
        var condicionIva = (CondicionIva)request.CondicionIva;
        bool esHumana = string.Equals(request.TipoPersona, "Humana", StringComparison.OrdinalIgnoreCase);

        try
        {
            Contribuyente creado = esHumana
                ? contribuyenteServicio.InscribirPersonaHumana(cuitUsuario, condicionIva)
                : contribuyenteServicio.RegistrarPersonaJuridica(cuitUsuario, request.Cuit ?? string.Empty,
                    request.RazonSocial ?? string.Empty, condicionIva);

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
            // Ya inscripto o CUIT ya registrado.
            return Conflict(new { mensaje = ex.Message });
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

        cuit = cuit.Trim();
        if (!contribuyenteServicio.Representa(User.ObtenerCuit(), cuit))
            return this.NoRepresenta(cuit);

        try
        {
            var puntoVenta = new PuntoVenta
            {
                CUITContribuyente = cuit,
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
    /// Se guarda solo su hash (la valida el SOAP) y las claves anteriores quedan revocadas.
    /// Se devuelve en texto plano una única vez.
    /// </summary>
    [HttpPost("{cuit}/api-key")]
    public async Task<ActionResult<ApiKeyResponse>> GenerarApiKey(string cuit, CancellationToken cancellationToken)
    {
        cuit = cuit.Trim();
        if (!contribuyenteServicio.Representa(User.ObtenerCuit(), cuit))
            return this.NoRepresenta(cuit);

        if (contribuyenteServicio.ConsultarPorCUIT(cuit) is null)
            return NotFound(new { mensaje = $"No existe un contribuyente con CUIT {cuit}." });

        var (nueva, revocadas) = await administradorApiKeys.RegenerarAsync(cuit, cancellationToken);

        var mensaje = "Guardá esta API Key en un lugar seguro: por seguridad se muestra una sola vez.";
        if (revocadas > 0)
            mensaje += " La anterior quedó revocada: el web service ya no la acepta.";

        return Ok(new ApiKeyResponse
        {
            Cuit = nueva.Cuit,
            ApiKey = nueva.Clave,
            FechaGeneracion = new DateTimeOffset(DateTime.SpecifyKind(nueva.FechaCreacion, DateTimeKind.Utc)),
            Mensaje = mensaje
        });
    }
}
