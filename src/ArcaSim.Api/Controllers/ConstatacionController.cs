using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using ArcaSim.Dominio.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ArcaSim.Api.Controllers;

[ApiController]
[Route("api/constatacion")]
public class ConstatacionController(ComprobanteServicio comprobanteServicio) : ControllerBase
{
    /// <summary>
    /// Verifica públicamente un comprobante por la URL de su código QR o por sus datos + CAE.
    /// </summary>
    [HttpPost]
    public ActionResult<ConstatacionResponse> Constatar([FromBody] ConstatarComprobanteRequest request)
    {
        if (request is null)
            return BadRequest(new { mensaje = "Los datos de constatación son obligatorios." });

        // Con la URL del código QR (la del portal o la de ARCA) se constata con los datos que trae.
        // Si no, con los datos del comprobante y su CAE.
        var resultado = !string.IsNullOrWhiteSpace(request.UrlQr)
            ? comprobanteServicio.ConstatarPorQr(request.UrlQr.Trim())
            : comprobanteServicio.Constatar(
                request.CuitEmisor?.Trim() ?? string.Empty,
                request.PuntoVentaNumero,
                (TipoComprobante)request.TipoComprobante,
                request.Numero,
                request.Cae?.Trim() ?? string.Empty);

        return Ok(Responder(resultado));
    }

    private ConstatacionResponse Responder(ResultadoConstatacion resultado) => new()
    {
        EsValido = resultado.EsValido,
        Mensaje = resultado.Mensaje,
        Comprobante = resultado.Comprobante is { } comprobante
            ? MapeadorRest.MapearComprobante(comprobante, comprobanteServicio.GenerarUrlQr(comprobante))
            : null
    };
}
