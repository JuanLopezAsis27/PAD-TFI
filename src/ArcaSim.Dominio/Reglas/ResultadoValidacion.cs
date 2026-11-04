using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Reglas;

public sealed class ResultadoValidacion
{
    public bool EsValido => Errores.Count == 0;
    public IReadOnlyList<ErrorValidacion> Errores { get; }
    private ResultadoValidacion(IReadOnlyList<ErrorValidacion> errores)
    {
        Errores = errores;
    }
    public static ResultadoValidacion Exito()
    {
        return new ResultadoValidacion(Array.Empty<ErrorValidacion>());
    }
    public static ResultadoValidacion ConErrores(params ErrorValidacion[] errores)
    {
        if (errores.Length == 0)
            throw new ArgumentException("Debe existir al menos un error.", nameof(errores));

        return new ResultadoValidacion(Array.AsReadOnly(errores.ToArray()));
    }
}