using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Interfaces;

public interface IContribuyenteRepositorio
{
    Contribuyente? ObtenerPorCUIT(string cuit);

    /// <summary>Contribuyentes con esos CUIT y sus puntos de venta, solo para leer, ordenados por nombre.</summary>
    IReadOnlyList<Contribuyente> ObtenerPorCuits(IReadOnlyCollection<string> cuits);

    void Agregar(Contribuyente contribuyente);

    void Actualizar(Contribuyente contribuyente);
}