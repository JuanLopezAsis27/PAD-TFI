using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public static class ValidadorCondicionIvaReceptor
{
    public static ErrorValidacion? Validar(CondicionIva? condicionReceptor, TipoComprobante tipoComprobante)
    {
        if (!condicionReceptor.HasValue)
        {
            return new ErrorValidacion("10246", "Falta informar la condición de IVA del receptor.");
        }

        if (!Enum.IsDefined(condicionReceptor.Value))
        {
            return new ErrorValidacion("10242", "La condición de IVA del receptor no es válida.");
        }

        return null;
    }
}