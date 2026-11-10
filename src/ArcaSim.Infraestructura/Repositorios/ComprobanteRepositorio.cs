using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Repositorios;

public class ComprobanteRepositorio(AppDbContext db) : IComprobanteRepositorio
{
    public Comprobante? ObtenerPorIdentificacion(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo, long numero)
    {
        return db.Comprobantes.FirstOrDefault(c =>
            c.CUITEmisor == cuitEmisor && c.PuntoVentaNumero == puntoVentaNumero &&
            c.Tipo == tipo && c.Numero == numero);
    }

    public Comprobante? ObtenerUltimoAutorizado(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo)
    {
        return db.Comprobantes
            .Where(c => c.CUITEmisor == cuitEmisor && c.PuntoVentaNumero == puntoVentaNumero &&
                c.Tipo == tipo &&
                (c.EstadoAutorizacion == EstadoAutorizacionComprobante.Autorizado ||
                 c.EstadoAutorizacion == EstadoAutorizacionComprobante.AutorizadoConObservaciones))
            .OrderByDescending(c => c.Numero)
            .FirstOrDefault();
    }

    public void Agregar(Comprobante comprobante)
    {
        db.Comprobantes.Add(comprobante);
        db.SaveChanges();
    }

    public void Actualizar(Comprobante comprobante)
    {
        // La clave es interna, así que solo se puede actualizar un comprobante leído de este repositorio.
        if (db.Entry(comprobante).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "El comprobante debe obtenerse del repositorio antes de actualizarlo.");
        }

        db.SaveChanges();
    }

    public void Eliminar(Comprobante comprobante)
    {
        db.Comprobantes.Remove(comprobante);
        db.SaveChanges();
    }
}
