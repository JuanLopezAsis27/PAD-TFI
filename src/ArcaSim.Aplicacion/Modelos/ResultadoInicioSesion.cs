using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Aplicacion.Modelos;

public enum EstadoInicioSesion
{
    Exitoso,

    /// <summary>CUIT inexistente o clave incorrecta: no se distingue, para no revelar qué CUIT tienen cuenta.</summary>
    CredencialesInvalidas,

    /// <summary>La clave está bloqueada por intentos fallidos hasta <see cref="ResultadoInicioSesion.BloqueadoHasta"/>.</summary>
    Bloqueado
}

public sealed record ResultadoInicioSesion(EstadoInicioSesion Estado, Usuario? Usuario = null,
    DateTime? BloqueadoHasta = null)
{
    public static ResultadoInicioSesion Exitoso(Usuario usuario) => new(EstadoInicioSesion.Exitoso, usuario);

    public static ResultadoInicioSesion CredencialesInvalidas() => new(EstadoInicioSesion.CredencialesInvalidas);

    public static ResultadoInicioSesion Bloqueado(DateTime hasta) => new(EstadoInicioSesion.Bloqueado, null, hasta);
}
