using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ArcaSim.Infraestructura.Repositorios;

public class UsuarioRepositorio(AppDbContext db) : IUsuarioRepositorio
{
    public Usuario? ObtenerPorCuit(string cuit)
    {
        return db.Usuarios.FirstOrDefault(u => u.Cuit == cuit);
    }

    public void Agregar(Usuario usuario)
    {
        db.Usuarios.Add(usuario);
        db.SaveChanges();
    }

    public void Actualizar(Usuario usuario)
    {
        if (db.Entry(usuario).State == EntityState.Detached)
            db.Usuarios.Update(usuario);

        db.SaveChanges();
    }

    public void AgregarRepresentacion(Representacion representacion)
    {
        db.Representaciones.Add(representacion);
        db.SaveChanges();
    }

    public bool Representa(string cuitUsuario, string cuitRepresentado)
    {
        return db.Representaciones.Any(r => r.CuitUsuario == cuitUsuario && r.CuitRepresentado == cuitRepresentado);
    }

    public IReadOnlyList<string> ObtenerCuitsRepresentados(string cuitUsuario)
    {
        return db.Representaciones
            .Where(r => r.CuitUsuario == cuitUsuario)
            .Select(r => r.CuitRepresentado)
            .ToList();
    }
}
