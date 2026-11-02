using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Reglas;

public sealed record ErrorValidacion(string codigo, string mensaje);