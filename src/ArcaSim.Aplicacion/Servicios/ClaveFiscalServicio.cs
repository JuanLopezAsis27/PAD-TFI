using System.Globalization;
using System.Net.Mail;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Servicios;

/// <summary>
/// Obtener la Clave Fiscal (registro, con validación de identidad en RENAPER) e ingresar con ella.
/// Ver docs/clave-fiscal.md.
/// </summary>
public class ClaveFiscalServicio(
    IUsuarioRepositorio usuarios,
    IRenaperCliente renaper,
    IHasheadorClave hasheador,
    TimeProvider reloj)
{
    /// <summary>Edad mínima para obtener la Clave Fiscal (y por lo tanto para inscribirse).</summary>
    public const int EdadMinima = 18;

    /// <summary>Intentos fallidos seguidos que bloquean la clave.</summary>
    public const int IntentosAntesDeBloquear = 5;

    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private const int LargoMaximoNombre = 100;
    private const int LargoMaximoEmail = 254;

    private static readonly string[] PrefijosPersonaHumana = ["20", "23", "24", "27"];

    /// <summary>
    /// Crea la cuenta de Clave Fiscal de una persona humana. Su CUIT/CUIL es prefijo + DNI + dígito
    /// verificador: se toma el DNI y se valida con RENAPER que el nombre y el apellido coincidan y que tenga
    /// la edad mínima. Todo lo demás se controla antes, porque cada consulta a RENAPER consume un crédito.
    /// </summary>
    /// <exception cref="ArgumentException">Datos inválidos (no se consulta a RENAPER).</exception>
    /// <exception cref="InvalidOperationException">Ya existe una cuenta para esa CUIT/CUIL (no se consulta a RENAPER).</exception>
    /// <exception cref="IdentidadNoValidadaException">RENAPER no validó a la persona.</exception>
    /// <exception cref="RenaperNoDisponibleException">RENAPER no dio una respuesta válida.</exception>
    public async Task<Usuario> RegistrarAsync(string cuit, string nombre, string apellido, string email,
        string clave, CancellationToken cancellationToken = default)
    {
        cuit = cuit?.Trim() ?? string.Empty;
        nombre = nombre?.Trim() ?? string.Empty;
        apellido = apellido?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (ValidadorCUIT.MotivoInvalida(cuit) is { } motivo)
            throw new ArgumentException(motivo);

        if (!PrefijosPersonaHumana.Contains(cuit[..2]))
        {
            throw new ArgumentException(
                "La Clave Fiscal la obtiene una persona humana: la CUIT/CUIL debe empezar con 20, 23, 24 o 27. " +
                "Las personas jurídicas se operan desde la cuenta de quien las representa.");
        }

        if (nombre.Length == 0 || apellido.Length == 0)
            throw new ArgumentException("El nombre y el apellido son obligatorios.");

        if (nombre.Length > LargoMaximoNombre || apellido.Length > LargoMaximoNombre)
            throw new ArgumentException($"El nombre y el apellido pueden tener hasta {LargoMaximoNombre} caracteres.");

        if (!EsEmailValido(email))
            throw new ArgumentException("El correo electrónico no es válido.");

        var incumplidos = ValidadorClaveFiscal.RequisitosIncumplidos(clave, cuit);
        if (incumplidos.Count > 0)
            throw new ArgumentException("La clave no cumple los requisitos. " + string.Join(" ", incumplidos));

        if (usuarios.ObtenerPorCuit(cuit) is not null)
            throw new InvalidOperationException("Ya existe una Clave Fiscal para esa CUIT/CUIL.");

        int dni = int.Parse(cuit.AsSpan(2, 8), CultureInfo.InvariantCulture);

        var resultado = await renaper.ValidarPersonaAsync(dni, nombre, apellido, EdadMinima, cancellationToken);

        if (!resultado.DatosCoinciden)
        {
            throw new IdentidadNoValidadaException(
                $"El nombre y el apellido no coinciden con los que RENAPER tiene para el DNI {dni}. " +
                "Escribilos tal como figuran en tu DNI.");
        }

        if (!resultado.CumpleEdadMinima)
            throw new IdentidadNoValidadaException($"Para obtener la Clave Fiscal tenés que tener al menos {EdadMinima} años.");

        var usuario = new Usuario
        {
            Cuit = cuit,
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            HashClave = hasheador.Hashear(clave),
            FechaAlta = reloj.GetUtcNow().UtcDateTime
        };

        usuarios.Agregar(usuario);
        return usuario;
    }

    /// <summary>
    /// Ingreso con CUIT/CUIL y clave. Después de <see cref="IntentosAntesDeBloquear"/> claves incorrectas
    /// seguidas, la clave queda bloqueada durante <see cref="DuracionBloqueo"/>, aunque después se escriba bien.
    /// </summary>
    public ResultadoInicioSesion IniciarSesion(string cuit, string clave)
    {
        cuit = cuit?.Trim() ?? string.Empty;
        clave ??= string.Empty;

        var usuario = cuit.Length == 0 ? null : usuarios.ObtenerPorCuit(cuit);
        if (usuario is null)
            return ResultadoInicioSesion.CredencialesInvalidas();

        var ahora = reloj.GetUtcNow().UtcDateTime;

        if (usuario.BloqueadoHasta is { } hasta && hasta > ahora)
            return ResultadoInicioSesion.Bloqueado(hasta);

        // Un bloqueo que ya venció no cuenta más.
        usuario.BloqueadoHasta = null;

        if (!hasheador.Verificar(clave, usuario.HashClave))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= IntentosAntesDeBloquear)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = ahora + DuracionBloqueo;
                usuarios.Actualizar(usuario);
                return ResultadoInicioSesion.Bloqueado(usuario.BloqueadoHasta.Value);
            }

            usuarios.Actualizar(usuario);
            return ResultadoInicioSesion.CredencialesInvalidas();
        }

        usuario.IntentosFallidos = 0;
        usuario.UltimoIngreso = ahora;
        usuarios.Actualizar(usuario);
        return ResultadoInicioSesion.Exitoso(usuario);
    }

    public Usuario? ObtenerUsuario(string cuit) => usuarios.ObtenerPorCuit(cuit);

    public IReadOnlyList<string> ObtenerCuitsRepresentados(string cuit) => usuarios.ObtenerCuitsRepresentados(cuit);

    private static bool EsEmailValido(string email)
    {
        return email.Length <= LargoMaximoEmail &&
            MailAddress.TryCreate(email, out var direccion) &&
            direccion.Address == email &&
            direccion.Host.Contains('.');
    }
}
