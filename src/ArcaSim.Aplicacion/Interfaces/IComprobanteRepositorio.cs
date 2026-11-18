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

    /// <summary>
    /// Comprobantes emitidos por cualquiera de esos CUIT, solo para leer: del más reciente al más viejo
    /// y, en la misma fecha, por punto de venta, tipo y número descendente.
    /// </summary>
    IReadOnlyList<Comprobante> ObtenerPorEmisores(IReadOnlyCollection<string> cuitsEmisores);

    void Agregar(Comprobante comprobante);
    void Actualizar(Comprobante comprobante);
    void Eliminar(Comprobante comprobante);
}
