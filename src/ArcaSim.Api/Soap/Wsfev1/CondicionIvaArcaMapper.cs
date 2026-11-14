
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Api.Soap.Wsfev1;

public static class CondicionIvaArcaMapper
{
    public static bool TryObtenerCondicion(
        int codigoArca,
        out CondicionIva condicion)
    {
        switch (codigoArca)
        {
            case 1:
                condicion = CondicionIva.ResponsableInscripto;
                return true;

            case 4:
                condicion = CondicionIva.Exento;
                return true;

            case 5:
                condicion = CondicionIva.ConsumidorFinal;
                return true;

            case 6:
                condicion = CondicionIva.Monotributista;
                return true;

            case 7:
                condicion = CondicionIva.SujetoNoCategorizado;
                return true;

            default:
                condicion = default;
                return false;
        }
    }

    public static bool TryObtenerCodigoArca(
        CondicionIva condicion,
        out int codigoArca)
    {
        switch (condicion)
        {
            case CondicionIva.ResponsableInscripto:
                codigoArca = 1;
                return true;

            case CondicionIva.Exento:
                codigoArca = 4;
                return true;

            case CondicionIva.ConsumidorFinal:
                codigoArca = 5;
                return true;

            case CondicionIva.Monotributista:
                codigoArca = 6;
                return true;

            case CondicionIva.SujetoNoCategorizado:
                codigoArca = 7;
                return true;

            default:
                codigoArca = 0;
                return false;
        }
    }
}
