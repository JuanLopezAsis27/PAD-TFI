using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Interfaces;

public interface IContribuyenteRepositorio
{
    Contribuyente? ObtenerPorCUIT(string cuit);

    void Agregar(Contribuyente contribuyente);

    void Actualizar(Contribuyente contribuyente);
}