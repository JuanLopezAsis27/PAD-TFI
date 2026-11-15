
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Interfaces;

public interface IPuntoVentaRepositorio
{
    // Obtener un punto de venta específico.
    PuntoVenta? ObtenerPorNumero(
        string cuitContribuyente,
        int numero);

    // Obtener todos los puntos de venta de una CUIT.
    IReadOnlyList<PuntoVenta> ObtenerPorCuit(
        string cuitContribuyente);

    // Agregar un punto de venta.
    void Agregar(PuntoVenta puntoVenta);

    // Actualizar un punto de venta.
    void Actualizar(PuntoVenta puntoVenta);
}
