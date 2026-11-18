using System.Security.Cryptography;
using System.Text;

namespace ArcaSim.Infraestructura.Seguridad;

public static class GeneradorApiKey
{
    public static string Generar()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToHexString(bytes);
    }

    public static string CalcularHash(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        byte[] bytes = Encoding.UTF8.GetBytes(apiKey);

        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}
