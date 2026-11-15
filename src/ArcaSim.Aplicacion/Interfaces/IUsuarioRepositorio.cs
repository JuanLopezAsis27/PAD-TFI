using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Interfaces;

/// <summary>Cuentas de Clave Fiscal y los contribuyentes que cada una representa.</summary>
public interface IUsuarioRepositorio
{
    Usuario? ObtenerPorCuit(string cuit);

    void Agregar(Usuario usuario);

    void Actualizar(Usuario usuario);

    void AgregarRepresentacion(Representacion representacion);

    /// <summary>Indica si la cuenta <paramref name="cuitUsuario"/> puede operar el CUIT <paramref name="cuitRepresentado"/>.</summary>
    bool Representa(string cuitUsuario, string cuitRepresentado);

    IReadOnlyList<string> ObtenerCuitsRepresentados(string cuitUsuario);
}
