using ArcaSim.Aplicacion.Interfaces;

namespace ArcaSim.Infraestructura.Seguridad;

public sealed class ValidadorApiKey : IValidadorApiKey
{
    private readonly IConsultaApiKeys _consultaApiKeys;

    public ValidadorApiKey(IConsultaApiKeys consultaApiKeys)
    {
        _consultaApiKeys = consultaApiKeys;
    }

    public ResultadoValidacionApiKey Validar(
        string apiKey,
        string cuit)
    {
        // 1. Comprobar que se informen los datos.
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ResultadoValidacionApiKey.Error(
                600,
                "La API key es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(cuit))
        {
            return ResultadoValidacionApiKey.Error(
                601,
                "La CUIT del emisor es obligatoria.");
        }

        // 2. Calcular el hash de la clave recibida.
        string hash = GeneradorApiKey.CalcularHash(apiKey);

        // 3. Buscar la clave en el almacenamiento.
        var claveRegistrada = _consultaApiKeys.ObtenerPorHash(hash);

        // 4. Verificar que exista y no esté revocada.
        if (claveRegistrada is null || claveRegistrada.Revocada)
        {
            return ResultadoValidacionApiKey.Error(
                600,
                "La API key es inválida o fue revocada.");
        }

        // 5. Comprobar que pertenezca a la CUIT.
        if (!string.Equals(
            claveRegistrada.Cuit,
            cuit,
            StringComparison.Ordinal))
        {
            return ResultadoValidacionApiKey.Error(
                601,
                "La CUIT informada no corresponde a la API key.");
        }

        // 6. Todas las validaciones fueron correctas.
        return ResultadoValidacionApiKey.Exito();
    }
}
