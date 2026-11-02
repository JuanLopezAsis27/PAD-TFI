using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Aplicacion.Interfaces;

public interface IComprobanteRepositorio
{
    Comprobante? ObtenerPorIdentificacion(string cuitEmisor, int puntoVentaNumero, TipoComprobante tipo,
        long numero);

    long ObtenerUltimoNumeroAutorizado(string cuitEmisor, int puntoVentaNumero, TipoComprobante tipo);

    void Agregar(Comprobante comprobante);
    void Actualizar(Comprobante comprobante);
}