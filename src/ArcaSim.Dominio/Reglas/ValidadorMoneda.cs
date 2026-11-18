using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Reglas;

public static class ValidadorMoneda
{
    // Códigos de la tabla de monedas de ARCA (FEParamGetTiposMonedas), no los ISO 4217.
    private static readonly HashSet<string> MonedasAdmitidas =
    [
        "PES", // Pesos argentinos
        "DOL", // Dólar estadounidense
        "060", // Euro
        "012", // Real
        "033", // Peso chileno
        "011"  // Peso uruguayo
    ];

    public static bool EsValida(string? moneda)
    {
        return !string.IsNullOrWhiteSpace(moneda) && MonedasAdmitidas.Contains(moneda);
    }
}
