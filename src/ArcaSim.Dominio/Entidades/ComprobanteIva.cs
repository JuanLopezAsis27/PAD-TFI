using ArcaSim.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Entidades;

public class ComprobanteIva
{
    public AlicuotaIva Alicuota { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal ImporteIVA { get; set; }
}