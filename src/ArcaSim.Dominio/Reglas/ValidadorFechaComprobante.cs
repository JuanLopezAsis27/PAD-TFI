using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorFechaComprobante
{
    public static bool EsValida(DateTime fechaComprobante, DateTime fechaGeneracion,
        ConceptoComprobante concepto, DateTime? fechaServicioDesde = null,
        DateTime? fechaServicioHasta = null, DateTime? fechaVencimientoPago = null)
    {
        if (fechaComprobante == default || fechaGeneracion == default)
            return false;

        if (!Enum.IsDefined(concepto))
            return false;

        var fecha = fechaComprobante.Date;
        var generacion = fechaGeneracion.Date;
        int margenDias = concepto == ConceptoComprobante.Productos ? 5 : 10;

        if (fecha < generacion.AddDays(-margenDias) || fecha > generacion.AddDays(margenDias))
            return false;

        if (concepto == ConceptoComprobante.Productos && fecha > generacion &&
            (fecha.Month != generacion.Month || fecha.Year != generacion.Year))
        {
            return false;
        }

        bool esServicio = concepto is ConceptoComprobante.Servicios
            or ConceptoComprobante.ProductosYServicios;

        if (!esServicio)
        {
            return fechaServicioDesde is null && fechaServicioHasta is null &&
                fechaVencimientoPago is null;
        }

        if (fechaServicioDesde is null || fechaServicioHasta is null || fechaVencimientoPago is null)
            return false;

        if (fechaVencimientoPago.Value.Date < fecha)
            return false;

        if (fechaServicioDesde.Value.Date > fechaServicioHasta.Value.Date)
            return false;

        return true;
    }
}