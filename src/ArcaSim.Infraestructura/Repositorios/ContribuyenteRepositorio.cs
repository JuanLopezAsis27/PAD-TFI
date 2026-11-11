using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Repositorios;

public class ContribuyenteRepositorio(AppDbContext db) : IContribuyenteRepositorio
{
    public Contribuyente? ObtenerPorCUIT(string cuit)
    {
        // Se cargan los puntos de venta: ValidadorRelacionPuntoVenta los necesita para aceptar un comprobante.
        return db.Contribuyentes
            .Include(c => c.PuntosVenta)
            .FirstOrDefault(c => c.CUIT == cuit);
    }

    public void Agregar(Contribuyente contribuyente)
    {
        db.Contribuyentes.Add(contribuyente);
        db.SaveChanges();
    }

    public void Actualizar(Contribuyente contribuyente)
    {
        if (db.Entry(contribuyente).State == EntityState.Detached)
            db.Contribuyentes.Update(contribuyente);

        db.SaveChanges();
    }
}
