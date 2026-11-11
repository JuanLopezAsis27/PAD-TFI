using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorCompatibilidadComprobante
{
    public static bool EsValido(TipoComprobante tipo, CondicionIva condicionEmisor,
        CondicionIva condicionReceptor)
    {
        return EsCompatibleEmisor(tipo, condicionEmisor) && EsCompatibleReceptor(tipo, condicionReceptor);
    }

    // Condiciones del receptor admitidas por clase de comprobante, según
    // FEParamGetCondicionIvaReceptor (RG 5616).
    public static bool EsCompatibleReceptor(TipoComprobante tipo, CondicionIva condicionReceptor)
    {
        return tipo switch
        {
            TipoComprobante.FacturaA or
            TipoComprobante.NotaDebitoA or
            TipoComprobante.NotaCreditoA
                => EsReceptorClaseA(condicionReceptor),

            TipoComprobante.FacturaB or
            TipoComprobante.NotaDebitoB or
            TipoComprobante.NotaCreditoB
                => EsReceptorClaseB(condicionReceptor),

            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC
                => EsReceptorClaseA(condicionReceptor) || EsReceptorClaseB(condicionReceptor),

            _ => false
        };
    }

    public static bool EsCompatibleEmisor(TipoComprobante tipo, CondicionIva condicionEmisor)
    {
        return tipo switch
        {
            TipoComprobante.FacturaA or
            TipoComprobante.NotaDebitoA or
            TipoComprobante.NotaCreditoA or
            TipoComprobante.FacturaB or
            TipoComprobante.NotaDebitoB or
            TipoComprobante.NotaCreditoB
                => condicionEmisor == CondicionIva.ResponsableInscripto,

            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC
                => EsMonotributo(condicionEmisor) || condicionEmisor == CondicionIva.Exento,

            _ => false
        };
    }

    private static bool EsReceptorClaseA(CondicionIva condicion)
    {
        return condicion == CondicionIva.ResponsableInscripto || EsMonotributo(condicion);
    }

    private static bool EsReceptorClaseB(CondicionIva condicion)
    {
        return condicion is CondicionIva.Exento
            or CondicionIva.ConsumidorFinal
            or CondicionIva.SujetoNoCategorizado
            or CondicionIva.ProveedorDelExterior
            or CondicionIva.ClienteDelExterior
            or CondicionIva.IvaLiberadoLey19640
            or CondicionIva.IvaNoAlcanzado;
    }

    private static bool EsMonotributo(CondicionIva condicion)
    {
        return condicion is CondicionIva.Monotributista
            or CondicionIva.MonotributistaSocial
            or CondicionIva.MonotributoTrabajadorIndependientePromovido;
    }
}
