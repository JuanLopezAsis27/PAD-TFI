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

    /// <summary>
    /// Dígito verificador que corresponde a los 10 primeros dígitos de una CUIT/CUIL, o null si el cálculo da
    /// 10: con ese prefijo el número no existe (ARCA cambia el prefijo, ver <see cref="MotivoInvalida"/>).
    /// </summary>
    public static int? CalcularDigitoVerificador(string primerosDiez)
    {
        if (primerosDiez is null || primerosDiez.Length != 10 || !primerosDiez.All(char.IsAsciiDigit))
            throw new ArgumentException("Se necesitan los 10 primeros dígitos de la CUIT.", nameof(primerosDiez));

        int suma = 0;

        for (int i = 0; i < 10; i++)
            suma += (primerosDiez[i] - '0') * Pesos[i];

        int digito = 11 - (suma % 11);

        return digito switch
        {
            11 => 0,
            10 => null,
            _ => digito
        };
    }

    // Cuando el dígito da 10, ARCA usa otro prefijo con un dígito fijo: 20 y 27 pasan a 23 (con 9 y 4),
    // y 30 pasa a 33 (con 9).
    private static readonly Dictionary<string, string> PrefijoAlternativo = new()
    {
        ["20"] = "23",
        ["27"] = "23",
        ["30"] = "33"
    };

    /// <summary>
    /// Explica, para mostrarle al usuario, por qué la CUIT/CUIL no es válida; si el problema es el dígito
    /// verificador, dice cuál corresponde. Devuelve null si es válida.
    /// </summary>
    public static string? MotivoInvalida(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return "Ingresá la CUIT/CUIL.";

        if (cuit.Length != 11 || !cuit.All(char.IsAsciiDigit))
            return "La CUIT/CUIL tiene que tener 11 dígitos, sin guiones ni espacios.";

        if (cuit.All(c => c == '0'))
            return "La CUIT/CUIL no puede ser todos ceros.";

        if (EsValido(cuit))
            return null;

        string prefijo = cuit[..2];
        string numero = cuit[2..10];
        int? digito = CalcularDigitoVerificador(cuit[..10]);

        if (digito is not null)
        {
            return $"El último dígito (el verificador) no corresponde: para {prefijo}-{numero} tiene que ser " +
                $"{digito}, o sea {Formatear(cuit[..10] + digito)}. Revisá que la CUIT/CUIL esté bien escrita.";
        }

        if (PrefijoAlternativo.TryGetValue(prefijo, out var alternativo) &&
            CalcularDigitoVerificador(alternativo + numero) is int digitoAlternativo)
        {
            return $"Con el prefijo {prefijo} ese número no tiene dígito verificador posible: en esos casos la " +
                $"CUIT/CUIL empieza con {alternativo}, o sea {Formatear(alternativo + numero + digitoAlternativo)}.";
        }

        return $"Con el prefijo {prefijo} ese número no tiene dígito verificador posible. Revisá la CUIT/CUIL.";
    }

    /// <summary>Formato con guiones: 20-40111222-8.</summary>
    public static string Formatear(string cuit) =>
        cuit.Length == 11 ? $"{cuit[..2]}-{cuit[2..10]}-{cuit[10]}" : cuit;
}