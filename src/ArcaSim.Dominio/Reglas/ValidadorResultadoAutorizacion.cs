using ArcaSim.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Reglas;

public static class ValidadorResultadoAutorizacion
{
    public static bool EsValido(ResultadoAutorizacion? resultado)
    {
        if (resultado is null)
            return false;

        if (!resultado.Autorizado)
            return string.IsNullOrWhiteSpace(resultado.CAE) && resultado.FechaVencimientoCAE == default;

        return resultado.CAE.Length == 14 && resultado.CAE.All(char.IsAsciiDigit) &&
               resultado.FechaVencimientoCAE != default;
    }
}