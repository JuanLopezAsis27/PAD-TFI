using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Entidades;

public sealed record ResultadoAutorizacion
{
    public string CAE { get; init; } = string.Empty;
    public DateOnly FechaVencimientoCAE { get; init; }
    public bool Autorizado { get; init; }
    public IReadOnlyList<string> Observaciones { get; init; } = Array.Empty<string>();
}