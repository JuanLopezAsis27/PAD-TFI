namespace ArcaSim.Aplicacion.Modelos;

/// <summary>Respuesta de RENAPER: solo es válida si los dos valores son verdaderos.</summary>
public sealed record ResultadoValidacionPersona(bool DatosCoinciden, bool CumpleEdadMinima)
{
    public bool EsValida => DatosCoinciden && CumpleEdadMinima;
}
