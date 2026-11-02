using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Dominio.Reglas;

public class ServicioQrComprobante
{
    public static string GenerarUrl(Comprobante? comprobante)
    {
        var datosQr = MapeadorDatosQrComprobante.Mapear(comprobante);
        return GeneradorQrComprobante.GenerarUrl(datosQr);
    }
}