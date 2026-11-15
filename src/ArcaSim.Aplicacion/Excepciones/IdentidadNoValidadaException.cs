namespace ArcaSim.Aplicacion.Excepciones;

/// <summary>
/// RENAPER respondió, pero no validó a la persona: el nombre y el apellido no coinciden con el DNI o
/// no cumple la edad mínima. Distinto de <see cref="RenaperNoDisponibleException"/>, donde no hubo respuesta.
/// </summary>
public class IdentidadNoValidadaException(string mensaje) : InvalidOperationException(mensaje);
