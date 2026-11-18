using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorAlicuotaIva
{
    private const decimal ToleranciaAbsoluta = 0.01m;
    private const decimal ToleranciaRelativa = 0.0001m;

    public static List<ErrorValidacion> Validar(ComprobanteIva? detalle,
        bool permiteBaseImponibleCero = false)
    {
        var errores = new List<ErrorValidacion>();

        if (detalle is null)
        {
            errores.Add(new ErrorValidacion("DOM-007","El detalle de alícuota de IVA no puede ser nulo."));
            return errores;
        }

        if (!Enum.IsDefined(typeof(AlicuotaIva), detalle.Alicuota))
        {
            errores.Add(new ErrorValidacion("10019","El identificador de alícuota de IVA es inválido."));
        }

        if (detalle.BaseImponible < 0
            || (!permiteBaseImponibleCero && detalle.BaseImponible == 0))
        {
            errores.Add(new ErrorValidacion("10020",
                "La base imponible debe ser mayor que cero, salvo las excepciones permitidas."));
        }

        if (detalle.ImporteIVA < 0)
        {
            errores.Add(new ErrorValidacion("10021","El importe de IVA no puede ser negativo."));
        }

        if (!Enum.IsDefined(typeof(AlicuotaIva), detalle.Alicuota) || detalle.BaseImponible < 0
            || (!permiteBaseImponibleCero && detalle.BaseImponible == 0) || detalle.ImporteIVA < 0)
        {
            return errores;
        }

        decimal porcentaje = ObtenerPorcentaje(detalle.Alicuota);

        decimal ivaEsperado = Math.Round(detalle.BaseImponible * porcentaje, 2,
            MidpointRounding.AwayFromZero);

        decimal diferencia = Math.Abs(detalle.ImporteIVA - ivaEsperado);

        bool cumpleTolerancia = diferencia <= ToleranciaAbsoluta
            || (ivaEsperado != 0 && diferencia / Math.Abs(ivaEsperado) <= ToleranciaRelativa);

        if (!cumpleTolerancia)
        {
            errores.Add(new ErrorValidacion("10051",
                "El importe de IVA no corresponde a la base imponible y la alícuota informadas."));
        }

        return errores;
    }

    private static decimal ObtenerPorcentaje(AlicuotaIva alicuota)
    {
        return alicuota switch
        {
            AlicuotaIva.Cero => 0m,
            AlicuotaIva.ReducidaDiezConCinco => 0.105m,
            AlicuotaIva.GeneralVeintiuno => 0.21m,
            AlicuotaIva.IncrementadaVeintisiete => 0.27m,
            AlicuotaIva.ReducidaCinco => 0.05m,
            AlicuotaIva.ReducidaDosConCinco => 0.025m,
            _ => throw new ArgumentOutOfRangeException(nameof(alicuota))
        };
    }
}