using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Entidades;

public class PuntoVenta
{
    public string CUITContribuyente { get; set; } = string.Empty;
    public int Numero { get; set; }
    public bool Habilitado { get; set; } = true;
    public Contribuyente? Contribuyente { get; set; }
    public List<Comprobante> Comprobantes { get; set; } = new();
}