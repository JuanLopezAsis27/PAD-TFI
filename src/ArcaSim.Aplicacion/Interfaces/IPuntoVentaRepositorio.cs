using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Interfaces;

public interface IPuntoVentaRepositorio
{
    PuntoVenta? ObtenerPorNumero(string cuitContribuyente, int numero);

    void Agregar(PuntoVenta puntoVenta);

    void Actualizar(PuntoVenta puntoVenta);
}