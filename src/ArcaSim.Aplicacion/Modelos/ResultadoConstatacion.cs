using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Modelos;

public class ResultadoConstatacion
{
    public bool EsValido { get; init; }
    public string Mensaje { get; init; } = string.Empty;

    /// <summary>El comprobante constatado, solo cuando es válido.</summary>
    public Comprobante? Comprobante { get; init; }
}
