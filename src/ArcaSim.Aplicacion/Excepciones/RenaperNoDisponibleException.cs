namespace ArcaSim.Aplicacion.Excepciones;

/// <summary>
/// RENAPER no dio una respuesta válida (sin conexión, API key inválida, sin saldo, respuesta inesperada).
/// No significa que la persona no exista: el alta se rechaza porque no se pudo validar.
/// </summary>
public class RenaperNoDisponibleException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);
