namespace ArcaSim.Dominio.Reglas;

/// <summary>
/// Requisitos de la Clave Fiscal según ARCA: entre 10 y 64 caracteres, al menos 2 números, 4 letras,
/// 1 mayúscula y 1 minúscula. Además, siguiendo sus consejos de seguridad, no puede contener la CUIT
/// ni el DNI del usuario.
/// </summary>
public static class ValidadorClaveFiscal
{
    public const int LargoMinimo = 10;
    public const int LargoMaximo = 64;

    /// <summary>
    /// Devuelve los requisitos que la clave no cumple, en el orden en que los muestra el portal.
    /// Si la lista está vacía, la clave es válida.
    /// </summary>
    /// <param name="cuit">CUIT/CUIL del usuario, para controlar que la clave no la contenga.</param>
    public static IReadOnlyList<string> RequisitosIncumplidos(string? clave, string? cuit = null)
    {
        var incumplidos = new List<string>();
        clave ??= string.Empty;

        if (clave.Length < LargoMinimo || clave.Length > LargoMaximo)
            incumplidos.Add($"Debe tener entre {LargoMinimo} y {LargoMaximo} caracteres.");

        if (clave.Count(char.IsAsciiDigit) < 2)
            incumplidos.Add("Debe tener al menos 2 números.");

        if (clave.Count(char.IsAsciiLetter) < 4)
            incumplidos.Add("Debe tener al menos 4 letras.");

        if (!clave.Any(char.IsAsciiLetterUpper))
            incumplidos.Add("Debe tener al menos 1 mayúscula.");

        if (!clave.Any(char.IsAsciiLetterLower))
            incumplidos.Add("Debe tener al menos 1 minúscula.");

        if (clave.Any(char.IsWhiteSpace))
            incumplidos.Add("No puede tener espacios.");

        if (ContieneDatosPersonales(clave, cuit))
            incumplidos.Add("No puede contener tu CUIT/CUIL ni tu número de DNI.");

        return incumplidos;
    }

    public static bool EsValida(string? clave, string? cuit = null) => RequisitosIncumplidos(clave, cuit).Count == 0;

    private static bool ContieneDatosPersonales(string clave, string? cuit)
    {
        if (string.IsNullOrEmpty(cuit) || cuit.Length != 11 || !cuit.All(char.IsAsciiDigit))
            return false;

        string dni = cuit.Substring(2, 8).TrimStart('0');
        return clave.Contains(cuit, StringComparison.Ordinal) ||
            (dni.Length > 0 && clave.Contains(dni, StringComparison.Ordinal));
    }
}
