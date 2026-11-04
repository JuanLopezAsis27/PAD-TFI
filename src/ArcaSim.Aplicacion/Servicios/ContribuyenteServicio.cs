using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Servicios;

public class ContribuyenteServicio
{
    private readonly IContribuyenteRepositorio _repositorio;

    public ContribuyenteServicio(IContribuyenteRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public void Registrar(Contribuyente contribuyente)
    {
        ArgumentNullException.ThrowIfNull(contribuyente);

        if (!ValidadorCUIT.EsValido(contribuyente.CUIT))
            throw new ArgumentException("El CUIT del contribuyente no es válido.");

        if (string.IsNullOrWhiteSpace(contribuyente.NombreORazonSocial))
            throw new ArgumentException("El nombre o razón social es obligatorio.");

        if (!Enum.IsDefined(contribuyente.TipoPersona))
            throw new ArgumentException("El tipo de persona no es válido.");

        if (!Enum.IsDefined(contribuyente.CondicionIva))
            throw new ArgumentException("La condición de IVA no es válida.");

        if (_repositorio.ObtenerPorCUIT(contribuyente.CUIT) is not null)
            throw new InvalidOperationException("Ya existe un contribuyente registrado con ese CUIT.");

        _repositorio.Agregar(contribuyente);
    }

    public Contribuyente? ConsultarPorCUIT(string cuit)
    {
        return _repositorio.ObtenerPorCUIT(cuit);
    }
}