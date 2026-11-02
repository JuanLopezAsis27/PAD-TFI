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
        return tipo switch
        {
            TipoComprobante.FacturaA or
            TipoComprobante.NotaDebitoA or
            TipoComprobante.NotaCreditoA
            => condicionEmisor == CondicionIva.ResponsableInscripto
            && (
                condicionReceptor == CondicionIva.ResponsableInscripto
                || condicionReceptor == CondicionIva.Monotributista
            ),

            TipoComprobante.FacturaB or
            TipoComprobante.NotaDebitoB or
            TipoComprobante.NotaCreditoB
            => condicionEmisor == CondicionIva.ResponsableInscripto
            && (
                condicionReceptor == CondicionIva.ConsumidorFinal
                || condicionReceptor == CondicionIva.Exento
                || condicionReceptor == CondicionIva.NoResponsable
                || condicionReceptor == CondicionIva.SujetoNoCategorizado
            ),

            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC
            => condicionEmisor == CondicionIva.Monotributista
            || condicionEmisor == CondicionIva.Exento,

            _ => false
        };
    }

    public static bool EsCompatibleReceptor(TipoComprobante tipo, CondicionIva condicionReceptor)
    {
        return tipo switch
        {
            TipoComprobante.FacturaA or
            TipoComprobante.NotaDebitoA or
            TipoComprobante.NotaCreditoA
                => condicionReceptor == CondicionIva.ResponsableInscripto
                || condicionReceptor == CondicionIva.Monotributista,

            TipoComprobante.FacturaB or
            TipoComprobante.NotaDebitoB or
            TipoComprobante.NotaCreditoB
                => condicionReceptor == CondicionIva.ConsumidorFinal
                || condicionReceptor == CondicionIva.Exento
                || condicionReceptor == CondicionIva.NoResponsable
                || condicionReceptor == CondicionIva.SujetoNoCategorizado,

            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC
                => condicionReceptor is CondicionIva.ResponsableInscripto
                or CondicionIva.Monotributista
                or CondicionIva.Exento
                or CondicionIva.NoResponsable
                or CondicionIva.ConsumidorFinal
                or CondicionIva.SujetoNoCategorizado,

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
                => condicionEmisor == CondicionIva.Monotributista
                    || condicionEmisor == CondicionIva.Exento,

            _ => false
        };
    }
}