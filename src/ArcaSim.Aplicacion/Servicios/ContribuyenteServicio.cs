using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Servicios;

/// <summary>
/// Alta de contribuyentes. Como en ARCA, se hace con la sesión de una cuenta de Clave Fiscal: la persona se
/// inscribe a sí misma o da de alta una persona jurídica, y queda como quien la representa. La identidad
/// ya se validó con RENAPER al crear la cuenta (ver <see cref="ClaveFiscalServicio"/>).
/// </summary>
public class ContribuyenteServicio
{
    private static readonly string[] PrefijosPersonaJuridica = ["30", "33", "34"];

    private readonly IContribuyenteRepositorio _repositorio;
    private readonly IUsuarioRepositorio _usuarios;

    public ContribuyenteServicio(IContribuyenteRepositorio repositorio, IUsuarioRepositorio usuarios)
    {
        _repositorio = repositorio;
        _usuarios = usuarios;
    }

    /// <summary>
    /// Alta de una persona jurídica sin representante (la usan los datos de prueba). Desde el portal se usa
    /// <see cref="RegistrarPersonaJuridica"/>, que deja asociada la cuenta que la da de alta.
    /// </summary>
    public void Registrar(Contribuyente contribuyente)
    {
        ArgumentNullException.ThrowIfNull(contribuyente);

        if (contribuyente.TipoPersona == TipoPersona.Humana)
        {
            throw new InvalidOperationException(
                "Las personas humanas se inscriben con InscribirPersonaHumana, desde su cuenta de Clave Fiscal.");
        }

        ValidarYAgregar(contribuyente);
    }

    /// <summary>
    /// Inscripción de la persona dueña de la cuenta, con el CUIT y el nombre que validó RENAPER al crearla
    /// (no se vuelve a consultar). Si su CUIT ya estaba dado de alta sin cuenta asociada, se le asocia.
    /// </summary>
    /// <exception cref="ArgumentException">La condición de IVA no es válida.</exception>
    /// <exception cref="InvalidOperationException">La cuenta no existe o ya está inscripta.</exception>
    public Contribuyente InscribirPersonaHumana(string cuitUsuario, CondicionIva condicionIva)
    {
        var usuario = ObtenerUsuario(cuitUsuario);

        if (!Enum.IsDefined(condicionIva))
            throw new ArgumentException("La condición de IVA no es válida.");

        var existente = _repositorio.ObtenerPorCUIT(usuario.Cuit);
        if (existente is not null)
        {
            if (_usuarios.Representa(usuario.Cuit, existente.CUIT))
                throw new InvalidOperationException("Ya estás inscripto como contribuyente.");

            AgregarRepresentacion(usuario.Cuit, existente.CUIT);
            return existente;
        }

        var contribuyente = new Contribuyente
        {
            CUIT = usuario.Cuit,
            NombreORazonSocial = usuario.NombreCompleto,
            TipoPersona = TipoPersona.Humana,
            CondicionIva = condicionIva
        };

        ValidarYAgregar(contribuyente);
        AgregarRepresentacion(usuario.Cuit, contribuyente.CUIT);
        return contribuyente;
    }

    /// <summary>
    /// Alta de una persona jurídica. No tiene clave propia: la opera la cuenta que la da de alta.
    /// </summary>
    /// <exception cref="ArgumentException">Datos inválidos.</exception>
    /// <exception cref="InvalidOperationException">La cuenta no existe o el CUIT ya está registrado.</exception>
    public Contribuyente RegistrarPersonaJuridica(string cuitUsuario, string cuit, string razonSocial,
        CondicionIva condicionIva)
    {
        var usuario = ObtenerUsuario(cuitUsuario);
        cuit = cuit?.Trim() ?? string.Empty;

        if (ValidadorCUIT.MotivoInvalida(cuit) is { } motivo)
            throw new ArgumentException(motivo);

        if (!PrefijosPersonaJuridica.Contains(cuit[..2]))
            throw new ArgumentException("El CUIT de una persona jurídica debe empezar con 30, 33 o 34.");

        var contribuyente = new Contribuyente
        {
            CUIT = cuit,
            NombreORazonSocial = razonSocial?.Trim() ?? string.Empty,
            TipoPersona = TipoPersona.Juridica,
            CondicionIva = condicionIva
        };

        ValidarYAgregar(contribuyente);
        AgregarRepresentacion(usuario.Cuit, contribuyente.CUIT);
        return contribuyente;
    }

    /// <summary>Indica si la cuenta <paramref name="cuitUsuario"/> puede operar el CUIT <paramref name="cuit"/>.</summary>
    public bool Representa(string cuitUsuario, string cuit) => _usuarios.Representa(cuitUsuario, cuit);

    public Contribuyente? ConsultarPorCUIT(string cuit)
    {
        return _repositorio.ObtenerPorCUIT(cuit);
    }

    private Usuario ObtenerUsuario(string cuitUsuario)
    {
        return _usuarios.ObtenerPorCuit(cuitUsuario?.Trim() ?? string.Empty)
            ?? throw new InvalidOperationException("No existe una cuenta de Clave Fiscal para esa CUIT/CUIL.");
    }

    private void AgregarRepresentacion(string cuitUsuario, string cuitRepresentado)
    {
        _usuarios.AgregarRepresentacion(new Representacion
        {
            CuitUsuario = cuitUsuario,
            CuitRepresentado = cuitRepresentado,
            FechaAlta = DateTime.UtcNow
        });
    }

    private void ValidarYAgregar(Contribuyente contribuyente)
    {
        if (!ValidadorCUIT.EsValido(contribuyente.CUIT))
            throw new ArgumentException("El CUIT del contribuyente no es válido.");

        if (string.IsNullOrWhiteSpace(contribuyente.NombreORazonSocial))
            throw new ArgumentException("El nombre o razón social es obligatorio.");

        if (!Enum.IsDefined(contribuyente.TipoPersona))
            throw new ArgumentException("El tipo de persona no es válido.");

        if (!Enum.IsDefined(contribuyente.CondicionIva))
            throw new ArgumentException("La condición de IVA no es válida.");

        if (_repositorio.ObtenerPorCUIT(contribuyente.CUIT) is not null)
            throw new InvalidOperationException("Ya existe un contribuyente registrado con ese CUIT.");

        _repositorio.Agregar(contribuyente);
    }
}
