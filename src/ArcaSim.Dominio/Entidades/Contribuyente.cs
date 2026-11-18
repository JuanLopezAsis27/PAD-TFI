using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Entidades;

public class Contribuyente
{
    public string CUIT { get; set; } = string.Empty;
    public string NombreORazonSocial { get; set; } = string.Empty;
    public TipoPersona TipoPersona { get; set; }
    public CondicionIva CondicionIva { get; set; }
    public List<PuntoVenta> PuntosVenta { get; set; } = new();
}