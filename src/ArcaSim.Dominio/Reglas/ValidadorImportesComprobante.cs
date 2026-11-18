using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorImportesComprobante
{
    public static List<ErrorValidacion> Validar(Comprobante comprobante)
    {
        var errores = new List<ErrorValidacion>();
        bool esTipoC = EsComprobanteTipoC(comprobante.Tipo);

        if (comprobante.ImporteNoGravado < 0)
        {
            errores.Add(new ErrorValidacion("10043",
                "El importe no gravado no puede ser menor a cero."));
        }
        else if (esTipoC && comprobante.ImporteNoGravado != 0)
        {
            errores.Add(new ErrorValidacion("10043",
                "Los comprobantes tipo C deben informar importe no gravado igual a cero."));
        }

        if (comprobante.ImporteExento < 0)
        {
            errores.Add(new ErrorValidacion("10044",
                "El importe exento no puede ser menor a cero."));
        }
        else if (esTipoC && comprobante.ImporteExento != 0)
        {
            errores.Add(new ErrorValidacion("10044",
                "Los comprobantes tipo C deben informar importe exento igual a cero."));
        }

        if (comprobante.ImporteNeto < 0)
        {
            errores.Add(new ErrorValidacion("10045",
                "El importe neto no puede ser menor a cero."));
        }

        if (comprobante.ImporteTributos < 0)
        {
            errores.Add(new ErrorValidacion("10046",
                "El importe de tributos no puede ser menor a cero."));
        }

        if (comprobante.ImporteIVA < 0)
        {
            errores.Add(new ErrorValidacion("10047",
                "El importe de IVA no puede ser menor a cero."));
        }
        else if (esTipoC && comprobante.ImporteIVA != 0)
        {
            errores.Add(new ErrorValidacion("10047",
                "Los comprobantes tipo C deben informar importe de IVA igual a cero."));
        }

        return errores;
    }

    private static bool EsComprobanteTipoC(TipoComprobante tipo)
    {
        return tipo is
            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC;
    }
}
