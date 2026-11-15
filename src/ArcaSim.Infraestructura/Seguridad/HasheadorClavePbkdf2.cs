using System.Globalization;
using System.Security.Cryptography;
using ArcaSim.Aplicacion.Interfaces;

namespace ArcaSim.Infraestructura.Seguridad;

/// <summary>
/// Hash de la Clave Fiscal con PBKDF2-HMAC-SHA512 y una salt aleatoria por clave (lo mismo que usa
/// ASP.NET Core Identity). Formato guardado: <c>v1$iteraciones$salt$hash</c>, con salt y hash en Base64,
/// así se pueden subir las iteraciones más adelante sin invalidar las claves existentes.
/// </summary>
public sealed class HasheadorClavePbkdf2 : IHasheadorClave
{
    private const string Version = "v1";
    private const int Iteraciones = 100_000;
    private const int BytesSalt = 16;
    private const int BytesHash = 32;
    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA512;

    public string Hashear(string clave)
    {
        ArgumentNullException.ThrowIfNull(clave);

        byte[] salt = RandomNumberGenerator.GetBytes(BytesSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, Algoritmo, BytesHash);

        return string.Join('$', Version, Iteraciones.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verificar(string clave, string hash)
    {
        if (clave is null || string.IsNullOrEmpty(hash))
            return false;

        string[] partes = hash.Split('$');
        if (partes.Length != 4 || partes[0] != Version ||
            !int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out int iteraciones) ||
            iteraciones <= 0)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(clave, salt, iteraciones, Algoritmo, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
