using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Tests.Aplicacion;

public class ContribuyenteServicioTests
{
    private const string CuitUsuario = "20401112228";

    private readonly RepositorioContribuyenteFalso _repositorio;
    private readonly RepositorioUsuarioFalso _usuarios;
    private readonly ContribuyenteServicio _servicio;

    public ContribuyenteServicioTests()
    {
        _repositorio = new RepositorioContribuyenteFalso();
        _usuarios = new RepositorioUsuarioFalso();
        _servicio = new ContribuyenteServicio(_repositorio, _usuarios);

        _usuarios.Agregar(new Usuario
        {
            Cuit = CuitUsuario,
            Nombre = "Lucas Matias",
            Apellido = "Gomez",
            Email = "lucas@example.com",
            HashClave = "hash"
        });
    }

    [Fact]
    public void ListarRepresentados_DevuelveSoloLosQueRepresentaLaCuentaOrdenadosPorNombre()
    {
        var zeta = _servicio.RegistrarPersonaJuridica(CuitUsuario, "30712345671", "Zeta S.A.",
            CondicionIva.ResponsableInscripto);
        var alfa = _servicio.RegistrarPersonaJuridica(CuitUsuario, "30709998885", "Alfa S.A.",
            CondicionIva.Exento);
        _repositorio.Agregar(new Contribuyente
        {
            CUIT = "30500010912",
            NombreORazonSocial = "Ajena S.A.",
            TipoPersona = TipoPersona.Juridica,
            CondicionIva = CondicionIva.ResponsableInscripto
        });

        var lista = _servicio.ListarRepresentados(CuitUsuario);

        Assert.Equal(new[] { alfa, zeta }, lista);
    }

    [Fact]
    public void ListarRepresentados_CuentaSinRepresentados_DevuelveVacio()
    {
        _repositorio.Agregar(CrearContribuyente());

        Assert.Empty(_servicio.ListarRepresentados(CuitUsuario));
        Assert.Empty(_servicio.ObtenerCuitsRepresentados(CuitUsuario));
    }

    [Fact]
    public void Registrar_ContribuyenteValido_AgregaRepositorio()
    {
        var contribuyente = CrearContribuyente();

        _servicio.Registrar(contribuyente);

        Assert.Contains(contribuyente, _repositorio.Contribuyentes);
    }

    [Fact]
    public void Registrar_ContribuyenteNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _servicio.Registrar(null!));
    }

    [Fact]
    public void Registrar_CUITInvalido_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.CUIT = "12345678901";

        Assert.Throws<ArgumentException>(() =>
            _servicio.Registrar(contribuyente));
    }

    [Fact]
    public void Registrar_SinNombreORazonSocial_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.NombreORazonSocial = " ";

        Assert.Throws<ArgumentException>(() =>
            _servicio.Registrar(contribuyente));
    }

    [Fact]
    public void Registrar_TipoPersonaInvalido_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.TipoPersona = (TipoPersona)999;

        Assert.Throws<ArgumentException>(() =>
            _servicio.Registrar(contribuyente));
    }

    [Fact]
    public void Registrar_CondicionIvaInvalida_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.CondicionIva = (CondicionIva)999;

        Assert.Throws<ArgumentException>(() => _servicio.Registrar(contribuyente));
    }

    [Fact]
    public void Registrar_CUITExistente_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        _repositorio.Agregar(contribuyente);

        var duplicado = CrearContribuyente();

        Assert.Throws<InvalidOperationException>(() => _servicio.Registrar(duplicado));
    }

    [Fact]
    public void Registrar_CUITInvalido_NoAgregaRepositorio()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.CUIT = "12345678901";
        Assert.Throws<ArgumentException>(() => _servicio.Registrar(contribuyente));
        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public void Registrar_PersonaHumana_LanzaExcepcionYNoAgrega()
    {
        var contribuyente = CrearContribuyente();
        contribuyente.TipoPersona = TipoPersona.Humana;

        Assert.Throws<InvalidOperationException>(() => _servicio.Registrar(contribuyente));
        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public void ConsultarPorCUIT_ContribuyenteExistente_DevuelveContribuyente()
    {
        var contribuyente = CrearContribuyente();
        _repositorio.Agregar(contribuyente);
        var resultado = _servicio.ConsultarPorCUIT(contribuyente.CUIT);
        Assert.Same(contribuyente, resultado);
    }

    [Fact]
    public void ConsultarPorCUIT_ContribuyenteInexistente_DevuelveNull()
    {
        var resultado = _servicio.ConsultarPorCUIT("20301234563");
        Assert.Null(resultado);
    }

    // ===== Inscripción de la persona dueña de la cuenta =====

    [Fact]
    public void InscribirPersonaHumana_CuentaExistente_InscribeConLosDatosDeLaCuentaYLaRepresenta()
    {
        var contribuyente = _servicio.InscribirPersonaHumana(CuitUsuario, CondicionIva.Monotributista);

        Assert.Contains(contribuyente, _repositorio.Contribuyentes);
        Assert.Equal(CuitUsuario, contribuyente.CUIT);
        Assert.Equal("Gomez, Lucas Matias", contribuyente.NombreORazonSocial);
        Assert.Equal(TipoPersona.Humana, contribuyente.TipoPersona);
        Assert.Equal(CondicionIva.Monotributista, contribuyente.CondicionIva);
        Assert.True(_servicio.Representa(CuitUsuario, CuitUsuario));
    }

    [Fact]
    public void InscribirPersonaHumana_YaInscripto_LanzaExcepcion()
    {
        _servicio.InscribirPersonaHumana(CuitUsuario, CondicionIva.Monotributista);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _servicio.InscribirPersonaHumana(CuitUsuario, CondicionIva.ResponsableInscripto));

        Assert.Contains("Ya estás inscripto", ex.Message);
        Assert.Single(_repositorio.Contribuyentes);
        Assert.Single(_usuarios.Representaciones);
    }

    [Fact]
    public void InscribirPersonaHumana_CuitYaRegistradoSinCuenta_LoAsociaALaCuenta()
    {
        // Por ejemplo, una persona humana dada de alta antes de que existiera la Clave Fiscal.
        var existente = new Contribuyente
        {
            CUIT = CuitUsuario,
            NombreORazonSocial = "Gomez, Lucas Matias",
            TipoPersona = TipoPersona.Humana,
            CondicionIva = CondicionIva.ResponsableInscripto
        };
        _repositorio.Agregar(existente);

        var contribuyente = _servicio.InscribirPersonaHumana(CuitUsuario, CondicionIva.Monotributista);

        Assert.Same(existente, contribuyente);
        Assert.Single(_repositorio.Contribuyentes);
        Assert.True(_servicio.Representa(CuitUsuario, CuitUsuario));
    }

    [Fact]
    public void InscribirPersonaHumana_CuentaInexistente_LanzaYNoInscribe()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _servicio.InscribirPersonaHumana("20301234563", CondicionIva.Monotributista));

        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public void InscribirPersonaHumana_CondicionIvaInvalida_LanzaYNoInscribe()
    {
        Assert.Throws<ArgumentException>(() => _servicio.InscribirPersonaHumana(CuitUsuario, (CondicionIva)999));

        Assert.Empty(_repositorio.Contribuyentes);
        Assert.Empty(_usuarios.Representaciones);
    }

    // ===== Alta de personas jurídicas representadas =====

    [Fact]
    public void RegistrarPersonaJuridica_DatosValidos_LaRegistraYQuedaRepresentada()
    {
        var juridica = _servicio.RegistrarPersonaJuridica(CuitUsuario, " 30712345671 ", " Comercio Demo S.A. ",
            CondicionIva.ResponsableInscripto);

        Assert.Contains(juridica, _repositorio.Contribuyentes);
        Assert.Equal("30712345671", juridica.CUIT);
        Assert.Equal("Comercio Demo S.A.", juridica.NombreORazonSocial);
        Assert.Equal(TipoPersona.Juridica, juridica.TipoPersona);
        Assert.True(_servicio.Representa(CuitUsuario, "30712345671"));
        Assert.False(_servicio.Representa(CuitUsuario, CuitUsuario));
    }

    [Theory]
    [InlineData("20301234563")] // CUIT de persona humana
    [InlineData("30712345672")] // dígito verificador incorrecto
    public void RegistrarPersonaJuridica_CuitNoValido_LanzaYNoRegistra(string cuit)
    {
        Assert.Throws<ArgumentException>(() =>
            _servicio.RegistrarPersonaJuridica(CuitUsuario, cuit, "Empresa", CondicionIva.ResponsableInscripto));

        Assert.Empty(_repositorio.Contribuyentes);
        Assert.Empty(_usuarios.Representaciones);
    }

    [Fact]
    public void RegistrarPersonaJuridica_SinRazonSocial_LanzaYNoRegistra()
    {
        Assert.Throws<ArgumentException>(() =>
            _servicio.RegistrarPersonaJuridica(CuitUsuario, "30712345671", " ", CondicionIva.ResponsableInscripto));

        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public void RegistrarPersonaJuridica_CuitYaRegistrado_LanzaYNoLaRepresenta()
    {
        _servicio.Registrar(new Contribuyente
        {
            CUIT = "30712345671",
            NombreORazonSocial = "Comercio Demo S.A.",
            TipoPersona = TipoPersona.Juridica,
            CondicionIva = CondicionIva.ResponsableInscripto
        });

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarPersonaJuridica(CuitUsuario, "30712345671", "Otra", CondicionIva.ResponsableInscripto));

        Assert.False(_servicio.Representa(CuitUsuario, "30712345671"));
    }

    [Fact]
    public void RegistrarPersonaJuridica_CuentaInexistente_LanzaYNoRegistra()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarPersonaJuridica("20301234563", "30712345671", "Empresa", CondicionIva.ResponsableInscripto));

        Assert.Empty(_repositorio.Contribuyentes);
    }

    private static Contribuyente CrearContribuyente()
    {
        return new Contribuyente
        {
            CUIT = "20301234563",
            NombreORazonSocial = "Empresa de prueba",
            TipoPersona = TipoPersona.Juridica,
            CondicionIva = CondicionIva.ResponsableInscripto
        };
    }
}

internal class RepositorioContribuyenteFalso : IContribuyenteRepositorio
{
    public List<Contribuyente> Contribuyentes { get; } = new();

    public Contribuyente? ObtenerPorCUIT(string cuit)
    {
        return Contribuyentes.FirstOrDefault(c => c.CUIT == cuit);
    }

    public IReadOnlyList<Contribuyente> ObtenerPorCuits(IReadOnlyCollection<string> cuits)
    {
        return Contribuyentes
            .Where(c => cuits.Contains(c.CUIT))
            .OrderBy(c => c.NombreORazonSocial)
            .ToList();
    }

    public void Agregar(Contribuyente contribuyente)
    {
        Contribuyentes.Add(contribuyente);
    }

    public void Actualizar(Contribuyente contribuyente)
    {
        int indice = Contribuyentes.FindIndex(c => c.CUIT == contribuyente.CUIT);

        if (indice >= 0)
            Contribuyentes[indice] = contribuyente;
    }
}

internal class RenaperClienteFalso : IRenaperCliente
{
    public ResultadoValidacionPersona Respuesta { get; set; } = new(DatosCoinciden: true, CumpleEdadMinima: true);
    public Exception? Error { get; set; }
    public List<(int Dni, string Nombre, string Apellido, int EdadMinima)> Consultas { get; } = new();

    public Task<ResultadoValidacionPersona> ValidarPersonaAsync(int dni, string nombre, string apellido,
        int edadMinima, CancellationToken cancellationToken = default)
    {
        Consultas.Add((dni, nombre, apellido, edadMinima));

        if (Error is not null)
            throw Error;

        return Task.FromResult(Respuesta);
    }
}
