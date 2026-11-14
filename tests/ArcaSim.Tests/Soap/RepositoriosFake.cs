
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Tests.Soap;

// =====================================================
// REPOSITORIO FALSO DE CONTRIBUYENTES
// =====================================================

public sealed class ContribuyenteRepositorioFake
    : IContribuyenteRepositorio
{
    private readonly Dictionary<string, Contribuyente> _contribuyentes
        = new();

    private readonly Func<Contribuyente, string> _obtenerCuit;

    public ContribuyenteRepositorioFake(
        Func<Contribuyente, string> obtenerCuit)
    {
        _obtenerCuit = obtenerCuit;
    }

    public Contribuyente? ObtenerPorCUIT(string cuit)
    {
        return _contribuyentes.GetValueOrDefault(cuit);
    }

    public void Agregar(Contribuyente contribuyente)
    {
        string cuit = _obtenerCuit(contribuyente);

        _contribuyentes.Add(cuit, contribuyente);
    }

    public void Actualizar(Contribuyente contribuyente)
    {
        string cuit = _obtenerCuit(contribuyente);

        _contribuyentes[cuit] = contribuyente;
    }
}

// =====================================================
// REPOSITORIO FALSO DE PUNTOS DE VENTA
// =====================================================

public sealed class PuntoVentaRepositorioFake
    : IPuntoVentaRepositorio
{
    private readonly Dictionary<(string Cuit, int Numero), PuntoVenta>
        _puntosVenta = new();

    private readonly Func<PuntoVenta, (string Cuit, int Numero)>
        _obtenerIdentificacion;

    public PuntoVentaRepositorioFake(
        Func<PuntoVenta, (string Cuit, int Numero)>
            obtenerIdentificacion)
    {
        _obtenerIdentificacion = obtenerIdentificacion;
    }

    public PuntoVenta? ObtenerPorNumero(
        string cuitContribuyente,
        int numero)
    {
        return _puntosVenta.GetValueOrDefault(
            (cuitContribuyente, numero));
    }

    public void Agregar(PuntoVenta puntoVenta)
    {
        var clave = _obtenerIdentificacion(puntoVenta);

        _puntosVenta.Add(clave, puntoVenta);
    }

    public void Actualizar(PuntoVenta puntoVenta)
    {
        var clave = _obtenerIdentificacion(puntoVenta);

        _puntosVenta[clave] = puntoVenta;
    }
}

// =====================================================
// REPOSITORIO FALSO DE COMPROBANTES
// =====================================================

public sealed class ComprobanteRepositorioFake
    : IComprobanteRepositorio
{
    private readonly Dictionary<
        (string Cuit, int PuntoVenta, TipoComprobante Tipo, long Numero),
        Comprobante> _comprobantes = new();

    public Comprobante? ObtenerPorIdentificacion(
        string cuitEmisor,
        int puntoVentaNumero,
        TipoComprobante tipo,
        long numero)
    {
        return _comprobantes.GetValueOrDefault(
            (cuitEmisor, puntoVentaNumero, tipo, numero));
    }

    public long ObtenerUltimoNumeroAutorizado(
        string cuitEmisor,
        int puntoVentaNumero,
        TipoComprobante tipo)
    {
        return _comprobantes.Values
            .Where(c =>
                c.CUITEmisor == cuitEmisor &&
                c.PuntoVentaNumero == puntoVentaNumero &&
                c.Tipo == tipo &&
                c.EstadoAutorizacion is
                    EstadoAutorizacionComprobante.Autorizado or
                    EstadoAutorizacionComprobante.AutorizadoConObservaciones)
            .Select(c => c.Numero)
            .DefaultIfEmpty(0)
            .Max();
    }

    public void Agregar(Comprobante comprobante)
    {
        var clave = ObtenerClave(comprobante);

        _comprobantes.Add(clave, comprobante);
    }

    public void Actualizar(Comprobante comprobante)
    {
        var clave = ObtenerClave(comprobante);

        _comprobantes[clave] = comprobante;
    }

    private static (
        string Cuit,
        int PuntoVenta,
        TipoComprobante Tipo,
        long Numero) ObtenerClave(Comprobante comprobante)
    {
        return (
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero);
    }
}
