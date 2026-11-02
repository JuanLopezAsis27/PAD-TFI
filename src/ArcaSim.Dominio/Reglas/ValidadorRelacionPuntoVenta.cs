using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorRelacionPuntoVenta
{
    public static bool EsValida(Contribuyente? contribuyente, PuntoVenta? puntoVenta)
    {
        if (contribuyente is null || puntoVenta is null)
            return false;

        if (string.IsNullOrWhiteSpace(contribuyente.CUIT) ||
            string.IsNullOrWhiteSpace(puntoVenta.CUITContribuyente))
            return false;

        if (puntoVenta.Contribuyente != null &&
            (string.IsNullOrWhiteSpace(puntoVenta.Contribuyente.CUIT) ||
             puntoVenta.Contribuyente.CUIT != contribuyente.CUIT))
            return false;

        if (contribuyente.CUIT != puntoVenta.CUITContribuyente)
            return false;

        if (contribuyente.PuntosVenta is null)
            return false;

        return contribuyente.PuntosVenta.Any(p => p.Numero == puntoVenta.Numero &&
            p.CUITContribuyente == puntoVenta.CUITContribuyente);
    }
}