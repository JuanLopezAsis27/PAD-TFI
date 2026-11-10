using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Modelos;

namespace ArcaSim.Aplicacion.Interfaces;

/// <summary>
/// Validación de identidad contra el RENAPER simulado de otro grupo (ver docs/renaper.md).
/// El servicio no devuelve los datos de la persona: solo confirma si los datos enviados coinciden.
/// </summary>
public interface IRenaperCliente
{
    /// <exception cref="RenaperNoDisponibleException">
    /// Si no se obtuvo una respuesta válida: sin conexión, API key inválida, sin saldo o respuesta inesperada.
    /// </exception>
    Task<ResultadoValidacionPersona> ValidarPersonaAsync(int dni, string nombre, string apellido, int edadMinima,
        CancellationToken cancellationToken = default);
}
