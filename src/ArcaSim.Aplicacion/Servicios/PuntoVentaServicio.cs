using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Servicios;

public class PuntoVentaServicio
{
    private readonly IPuntoVentaRepositorio _puntoVentaRepositorio;
    private readonly IContribuyenteRepositorio _contribuyenteRepositorio;

    public PuntoVentaServicio(IPuntoVentaRepositorio puntoVentaRepositorio,
        IContribuyenteRepositorio contribuyenteRepositorio)
    {
        _puntoVentaRepositorio = puntoVentaRepositorio;
        _contribuyenteRepositorio = contribuyenteRepositorio;
    }

    public void Registrar(PuntoVenta puntoVenta)
    {
        ArgumentNullException.ThrowIfNull(puntoVenta);

        if (!ValidadorCUIT.EsValido(puntoVenta.CUITContribuyente))
            throw new ArgumentException("El CUIT del contribuyente no es válido.");

        if (puntoVenta.Numero <= 0)
            throw new ArgumentException("El número del punto de venta debe ser positivo.");

        var contribuyente = _contribuyenteRepositorio.ObtenerPorCUIT(puntoVenta.CUITContribuyente);

        if (contribuyente is null)
            throw new InvalidOperationException("No existe el contribuyente indicado.");

        if (_puntoVentaRepositorio.ObtenerPorNumero(
            puntoVenta.CUITContribuyente, puntoVenta.Numero) is not null)
        {
            throw new InvalidOperationException(
                "El contribuyente ya tiene registrado ese punto de venta.");
        }

        puntoVenta.Contribuyente = contribuyente;

        if (!contribuyente.PuntosVenta.Contains(puntoVenta))
            contribuyente.PuntosVenta.Add(puntoVenta);

        _puntoVentaRepositorio.Agregar(puntoVenta);
        _contribuyenteRepositorio.Actualizar(contribuyente);
    }

    public PuntoVenta? ConsultarPorNumero(string cuitContribuyente, int numero)
    {
        return _puntoVentaRepositorio.ObtenerPorNumero(cuitContribuyente, numero);
    }
}