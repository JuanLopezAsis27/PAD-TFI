using ArcaSim.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ArcaSim.Aplicacion.Servicios;

public class GeneradorCaeSimulado
{
    private const int DiasVigenciaCAE = 10;

    public ResultadoAutorizacion Generar(DateOnly fechaComprobante)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(8);
        long numero = BitConverter.ToInt64(bytes) & long.MaxValue;

        numero = 10_000_000_000_000 + numero % 90_000_000_000_000;

        return new ResultadoAutorizacion
        {
            CAE = numero.ToString(),
            FechaVencimientoCAE = fechaComprobante.AddDays(DiasVigenciaCAE),
            Autorizado = true
        };
    }
}