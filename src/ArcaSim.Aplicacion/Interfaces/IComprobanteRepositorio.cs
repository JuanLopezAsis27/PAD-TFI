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

    /// <summary>
    /// Devuelve el comprobante autorizado (con o sin observaciones) de mayor número para ese emisor,
    /// punto de venta y tipo, o null si no hay ninguno. Los pendientes y rechazados no cuentan.
    /// </summary>
    Comprobante? ObtenerUltimoAutorizado(string cuitEmisor, int puntoVentaNumero, TipoComprobante tipo);

    void Agregar(Comprobante comprobante);
    void Actualizar(Comprobante comprobante);
    void Eliminar(Comprobante comprobante);
}
