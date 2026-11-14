using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Repositorios;

public class PuntoVentaRepositorio(AppDbContext db) : IPuntoVentaRepositorio
{
    public PuntoVenta? ObtenerPorNumero(string cuitContribuyente, int numero)
    {
        return db.PuntosVenta.FirstOrDefault(p =>
            p.CUITContribuyente == cuitContribuyente && p.Numero == numero);
    }

    public void Agregar(PuntoVenta puntoVenta)
    {
        db.PuntosVenta.Add(puntoVenta);
        db.SaveChanges();
    }

    public void Actualizar(PuntoVenta puntoVenta)
    {
        if (db.Entry(puntoVenta).State == EntityState.Detached)
            db.PuntosVenta.Update(puntoVenta);

        db.SaveChanges();
    }
}
