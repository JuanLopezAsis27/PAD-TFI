using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Tests.Aplicacion;

internal class RepositorioUsuarioFalso : IUsuarioRepositorio
{
    public List<Usuario> Usuarios { get; } = new();
    public List<Representacion> Representaciones { get; } = new();
    public int Actualizaciones { get; private set; }

    public Usuario? ObtenerPorCuit(string cuit) => Usuarios.FirstOrDefault(u => u.Cuit == cuit);

    public void Agregar(Usuario usuario) => Usuarios.Add(usuario);

    public void Actualizar(Usuario usuario) => Actualizaciones++;

    public void AgregarRepresentacion(Representacion representacion) => Representaciones.Add(representacion);

    public bool Representa(string cuitUsuario, string cuitRepresentado) =>
        Representaciones.Any(r => r.CuitUsuario == cuitUsuario && r.CuitRepresentado == cuitRepresentado);

    public IReadOnlyList<string> ObtenerCuitsRepresentados(string cuitUsuario) =>
        Representaciones.Where(r => r.CuitUsuario == cuitUsuario).Select(r => r.CuitRepresentado).ToList();
}

/// <summary>Hash trivial para los tests de la aplicación (el real se prueba en HasheadorClavePbkdf2Tests).</summary>
internal class HasheadorClaveFalso : IHasheadorClave
{
    public string Hashear(string clave) => "hash:" + clave;

    public bool Verificar(string clave, string hash) => hash == "hash:" + clave;
}

internal class RelojFalso(DateTimeOffset ahora) : TimeProvider
{
    public DateTimeOffset Ahora { get; set; } = ahora;

    public override DateTimeOffset GetUtcNow() => Ahora;
}
