using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class MapeadorDatosQrComprobante
{
    public static DatosQrComprobante Mapear(Comprobante? comprobante)
    {
        ArgumentNullException.ThrowIfNull(comprobante);

        bool estaAutorizado =
            comprobante.EstadoAutorizacion == EstadoAutorizacionComprobante.Autorizado ||
            comprobante.EstadoAutorizacion == EstadoAutorizacionComprobante.AutorizadoConObservaciones;

        if (!estaAutorizado)
            throw new InvalidOperationException(
                "Solo se pueden generar datos QR para comprobantes autorizados.");

        if (string.IsNullOrWhiteSpace(comprobante.CAE) || comprobante.FechaVencimientoCAE is null)
        {
            throw new InvalidOperationException(
                "El comprobante autorizado debe tener CAE y fecha de vencimiento.");
        }

        if (comprobante.Cotizacion is null)
        {
            throw new InvalidOperationException(
                "El comprobante debe tener una cotización informada para generar el QR.");
        }

        return new DatosQrComprobante
        {
            Fecha = DateOnly.FromDateTime(comprobante.Fecha),
            CUITEmisor = comprobante.CUITEmisor,
            PuntoVenta = comprobante.PuntoVentaNumero,
            TipoComprobante = (int)comprobante.Tipo,
            NumeroComprobante = comprobante.Numero,
            ImporteTotal = comprobante.ImporteTotal,
            Moneda = comprobante.Moneda,
            Cotizacion = comprobante.Cotizacion.Value,
            TipoDocumentoReceptor = string.IsNullOrWhiteSpace(comprobante.NumeroDocumentoReceptor)
                ? null : (int)comprobante.TipoDocumentoReceptor,
            NumeroDocumentoReceptor = string.IsNullOrWhiteSpace(comprobante.NumeroDocumentoReceptor)
                ? null : comprobante.NumeroDocumentoReceptor,
            TipoAutorizacion = "E",
            CodigoAutorizacion = comprobante.CAE
        };
    }
}