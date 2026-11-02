using System;
using System.Collections.Generic;
using System.Text;
public static class ValidadorMoneda
{
    private static readonly HashSet<string> MonedasAdmitidas = ["PES", "DOL", "EUR", "BRL", "CLP", "UYU"];

    public static bool EsValida(string? moneda)
    {
        return !string.IsNullOrWhiteSpace(moneda) && MonedasAdmitidas.Contains(moneda);
    }
}