using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorCUIT
{
    private static readonly int[] Pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    public static bool EsValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return false;

        if (cuit.Length != 11 || !cuit.All(char.IsAsciiDigit) || cuit.All(c => c == '0'))
            return false;

        int suma = 0;

        for (int i = 0; i < 10; i++)
            suma += (cuit[i] - '0') * Pesos[i];

        int digitoCalculado = 11 - (suma % 11);

        if (digitoCalculado == 11)
            digitoCalculado = 0;

        if (digitoCalculado == 10)
            return false;

        return cuit[10] - '0' == digitoCalculado;
    }
}