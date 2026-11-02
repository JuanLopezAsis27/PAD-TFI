using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Enums;

public enum EstadoAutorizacionComprobante
{
    Pendiente,
    Autorizado,
    AutorizadoConObservaciones,
    Rechazado
}