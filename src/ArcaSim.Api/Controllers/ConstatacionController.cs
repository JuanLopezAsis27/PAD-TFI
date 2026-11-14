using System.Text;
using System.Text.Json;
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

        // 1. Si se envía la URL del código QR de ARCA, se valida directamente con ConstatarPorQr.
        if (!string.IsNullOrWhiteSpace(request.UrlQr))
        {
            var urlLimpia = request.UrlQr.Trim();
            var resultadoQr = comprobanteServicio.ConstatarPorQr(urlLimpia);

            ComprobanteResponse? comprobanteDto = null;
            if (resultadoQr.EsValido && IntentarExtraerIdentificacionQr(urlLimpia, out var cuit, out var pv, out var tipo, out var nro))
            {
                var entidad = comprobanteServicio.Consultar(cuit, pv, tipo, nro);
                if (entidad is not null)
                    comprobanteDto = MapeadorRest.MapearComprobante(entidad);
            }

            return Ok(new ConstatacionResponse
            {
                EsValido = resultadoQr.EsValido,
                Mensaje = resultadoQr.Mensaje,
                Comprobante = comprobanteDto
            });
        }

        // 2. Si no hay URL de QR, se valida por los datos del comprobante y su CAE.
        var cuitEmisor = request.CuitEmisor?.Trim() ?? string.Empty;
        var tipoComprobante = (TipoComprobante)request.TipoComprobante;
        var cae = request.Cae?.Trim() ?? string.Empty;

        var resultado = comprobanteServicio.Constatar(
            cuitEmisor,
            request.PuntoVentaNumero,
            tipoComprobante,
            request.Numero,
            cae);

        ComprobanteResponse? detalle = null;
        if (resultado.EsValido)
        {
            var comprobante = comprobanteServicio.Consultar(
                cuitEmisor,
                request.PuntoVentaNumero,
                tipoComprobante,
                request.Numero);

            if (comprobante is not null)
                detalle = MapeadorRest.MapearComprobante(comprobante);
        }

        return Ok(new ConstatacionResponse
        {
            EsValido = resultado.EsValido,
            Mensaje = resultado.Mensaje,
            Comprobante = detalle
        });
    }

    private static bool IntentarExtraerIdentificacionQr(
        string urlQr,
        out string cuit,
        out int puntoVenta,
        out TipoComprobante tipo,
        out long numero)
    {
        cuit = string.Empty;
        puntoVenta = 0;
        tipo = default;
        numero = 0;

        try
        {
            var uri = new Uri(urlQr);
            var paramP = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .FirstOrDefault(p => p.Length == 2 && p[0] == "p");

            if (paramP is null)
                return false;

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(paramP[1])));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            cuit = root.GetProperty("cuit").GetRawText().Trim('"');
            puntoVenta = root.GetProperty("ptoVta").GetInt32();
            tipo = (TipoComprobante)root.GetProperty("tipoCmp").GetInt32();
            numero = root.GetProperty("nroCmp").GetInt64();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

