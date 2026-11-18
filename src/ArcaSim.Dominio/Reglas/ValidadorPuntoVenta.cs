using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorPuntoVenta
{
    public static bool EsValido(PuntoVenta? puntoVenta, Comprobante? comprobante)
    {
        if (puntoVenta == null || comprobante == null)
            return false;

        if (!puntoVenta.Habilitado || puntoVenta.Numero <= 0)
            return false;

        if(puntoVenta.Numero != comprobante.PuntoVentaNumero ||
            puntoVenta.CUITContribuyente != comprobante.CUITEmisor)
            return false;

        return true;
    }
}