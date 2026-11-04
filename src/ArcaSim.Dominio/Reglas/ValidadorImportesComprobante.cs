using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorImportesComprobante
{
    public static List<ErrorValidacion> Validar(Comprobante comprobante)
    {
        var errores = new List<ErrorValidacion>();

        if (comprobante.ImporteNoGravado < 0)
        {
            errores.Add(new ErrorValidacion("10043",
                "El importe no gravado no puede ser menor a cero."));
        }

        if (comprobante.ImporteExento < 0)
        {
            errores.Add(new ErrorValidacion("10044",
                "El importe exento no puede ser menor a cero."));
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

        return errores;
    }
}