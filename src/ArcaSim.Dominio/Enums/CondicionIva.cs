using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Enums;

// Los valores son los IDs de ARCA (FEParamGetCondicionIvaReceptor, RG 5616), para que el
// campo CondicionIVAReceptorId del web service se traduzca directo.
public enum CondicionIva
{
    ResponsableInscripto = 1,
    Exento = 4,
    ConsumidorFinal = 5,
    Monotributista = 6,
    SujetoNoCategorizado = 7,
    ProveedorDelExterior = 8,
    ClienteDelExterior = 9,
    IvaLiberadoLey19640 = 10,
    MonotributistaSocial = 13,
    IvaNoAlcanzado = 15,
    MonotributoTrabajadorIndependientePromovido = 16
}
