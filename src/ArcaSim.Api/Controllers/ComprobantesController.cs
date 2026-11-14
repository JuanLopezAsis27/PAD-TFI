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
[Route("api/comprobantes")]
public class ComprobantesController(
    ComprobanteServicio comprobanteServicio,
    AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista los comprobantes emitidos por un contribuyente (?cuit=...).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ComprobanteResponse>>> ListarPorCuit(
        [FromQuery] string? cuit,
        CancellationToken cancellationToken)
    {
        var query = db.Comprobantes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cuit))
        {
            var cuitLimpio = cuit.Trim();
            query = query.Where(c => c.CUITEmisor == cuitLimpio);
        }

        var comprobantes = await query
            .OrderByDescending(c => c.Fecha)
            .ThenBy(c => c.PuntoVentaNumero)
            .ThenBy(c => c.Tipo)
            .ThenByDescending(c => c.Numero)
            .ToListAsync(cancellationToken);

        return Ok(comprobantes.Select(MapeadorRest.MapearComprobante).ToList());
    }

    /// <summary>
    /// Obtiene un comprobante específico por su identificación (CUIT, punto de venta, tipo y número).
    /// </summary>
    [HttpGet("{cuit}/{puntoVenta:int}/{tipo:int}/{numero:long}")]
    public ActionResult<ComprobanteResponse> ObtenerDetalle(
        string cuit,
        int puntoVenta,
        int tipo,
        long numero)
    {
        var comprobante = comprobanteServicio.Consultar(
            cuit.Trim(),
            puntoVenta,
            (TipoComprobante)tipo,
            numero);

        if (comprobante is null)
            return NotFound(new { mensaje = "No se encontró el comprobante solicitado." });

        return Ok(MapeadorRest.MapearComprobante(comprobante));
    }

    /// <summary>
    /// Devuelve el último número de comprobante autorizado para ese emisor, punto de venta y tipo.
    /// </summary>
    [HttpGet("ultimo-autorizado")]
    public ActionResult ObtenerUltimoAutorizado(
        [FromQuery] string cuit,
        [FromQuery] int puntoVenta,
        [FromQuery] int tipo)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return BadRequest(new { mensaje = "El parámetro 'cuit' es obligatorio." });

        var ultimo = comprobanteServicio.ConsultarUltimoNumeroAutorizado(
            cuit.Trim(),
            puntoVenta,
            (TipoComprobante)tipo);

        return Ok(new
        {
            cuit = cuit.Trim(),
            puntoVenta,
            tipoComprobante = tipo,
            ultimoNumeroAutorizado = ultimo,
            proximoNumero = ultimo + 1
        });
    }

    /// <summary>
    /// Emisión manual de un comprobante desde el portal → valida según reglas de ARCA y otorga CAE.
    /// </summary>
    [HttpPost]
    public ActionResult<ComprobanteResponse> Emitir([FromBody] EmitirComprobanteRequest request)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos del comprobante son obligatorios." });

        var cuitEmisor = request.CuitEmisor?.Trim() ?? string.Empty;
        var tipo = (TipoComprobante)request.TipoComprobante;
        var fechaHoy = DateTime.Today;
        var fechaEmision = request.Fecha?.Date ?? fechaHoy;

        long numero = request.Numero is > 0
            ? request.Numero.Value
            : comprobanteServicio.ConsultarUltimoNumeroAutorizado(cuitEmisor, request.PuntoVentaNumero, tipo) + 1;

        var comprobante = new Comprobante
        {
            CUITEmisor = cuitEmisor,
            PuntoVentaNumero = request.PuntoVentaNumero,
            Tipo = tipo,
            Numero = numero,
            Concepto = (ConceptoComprobante)request.Concepto,
            Fecha = fechaEmision,
            FechaServicioDesde = request.FechaServicioDesde?.Date,
            FechaServicioHasta = request.FechaServicioHasta?.Date,
            FechaVencimientoPago = request.FechaVencimientoPago?.Date,
            TipoDocumentoReceptor = (TipoDocumento)request.TipoDocumentoReceptor,
            NumeroDocumentoReceptor = request.NumeroDocumentoReceptor?.Trim() ?? string.Empty,
            CondicionIvaReceptor = (CondicionIva)request.CondicionIvaReceptor,
            ImporteNeto = request.ImporteNeto,
            ImporteNoGravado = request.ImporteNoGravado,
            ImporteExento = request.ImporteExento,
            ImporteTributos = request.ImporteTributos,
            ImporteIVA = request.ImporteIva,
            ImporteTotal = request.ImporteTotal,
            Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "PES" : request.Moneda.Trim().ToUpperInvariant(),
            Cotizacion = request.Cotizacion,
            CancelaEnMismaMonedaExtranjera = request.CancelaEnMismaMonedaExtranjera,
            AlicuotasIva = request.AlicuotasIva.Select(a => new ComprobanteIva
            {
                Alicuota = (AlicuotaIva)a.AlicuotaId,
                BaseImponible = a.BaseImponible,
                ImporteIVA = a.ImporteIva
            }).ToList()
        };

        var validacion = comprobanteServicio.Registrar(comprobante, fechaHoy);
        if (!validacion.EsValido)
        {
            var respuestaRechazada = MapeadorRest.MapearComprobante(comprobante);
            respuestaRechazada.EstadoAutorizacion = EstadoAutorizacionComprobante.Rechazado.ToString();
            respuestaRechazada.Errores = validacion.Errores.Select(e => new ErrorValidacionDto
            {
                Codigo = e.codigo,
                Descripcion = e.mensaje
            }).ToList();

            return UnprocessableEntity(respuestaRechazada);
        }

        comprobanteServicio.AutorizarSimuladamente(
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero);

        var autorizado = comprobanteServicio.Consultar(
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero) ?? comprobante;

        return CreatedAtAction(
            nameof(ObtenerDetalle),
            new
            {
                cuit = autorizado.CUITEmisor,
                puntoVenta = autorizado.PuntoVentaNumero,
                tipo = (int)autorizado.Tipo,
                numero = autorizado.Numero
            },
            MapeadorRest.MapearComprobante(autorizado));
    }
}

