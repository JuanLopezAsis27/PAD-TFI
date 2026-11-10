using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

public class ContribuyenteServicioTests
{
    private readonly RepositorioContribuyenteFalso _repositorio;
    private readonly RenaperClienteFalso _renaper;
    private readonly ContribuyenteServicio _servicio;

    public ContribuyenteServicioTests()
    {
        _repositorio = new RepositorioContribuyenteFalso();
        _renaper = new RenaperClienteFalso();
        _servicio = new ContribuyenteServicio(_repositorio, _renaper);
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

    [Fact]
    public async Task RegistrarPersonaHumana_DatosValidados_RegistraConApellidoYNombre()
    {
        var contribuyente = await _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez",
            CondicionIva.Monotributista);

        Assert.Contains(contribuyente, _repositorio.Contribuyentes);
        Assert.Equal("20401112228", contribuyente.CUIT);
        Assert.Equal("Gomez, Lucas Matias", contribuyente.NombreORazonSocial);
        Assert.Equal(TipoPersona.Humana, contribuyente.TipoPersona);
        Assert.Equal(CondicionIva.Monotributista, contribuyente.CondicionIva);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_ConsultaARenaperElDniDelCuitYLaEdadMinima()
    {
        await _servicio.RegistrarPersonaHumanaAsync("20401112228", "  Lucas Matias ", " Gomez",
            CondicionIva.Monotributista);

        var consulta = Assert.Single(_renaper.Consultas);
        Assert.Equal(40111222, consulta.Dni);
        Assert.Equal("Lucas Matias", consulta.Nombre);
        Assert.Equal("Gomez", consulta.Apellido);
        Assert.Equal(ContribuyenteServicio.EdadMinimaPersonaHumana, consulta.EdadMinima);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_DniConCeroAdelante_ConsultaElNumeroSinCeros()
    {
        await _servicio.RegistrarPersonaHumanaAsync("20051234562", "Ana", "Gomez", CondicionIva.Monotributista);

        Assert.Equal(5123456, Assert.Single(_renaper.Consultas).Dni);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_DatosNoCoinciden_LanzaExcepcionYNoRegistra()
    {
        _renaper.Respuesta = new ResultadoValidacionPersona(DatosCoinciden: false, CumpleEdadMinima: true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", "Otro", "Nombre", CondicionIva.Monotributista));

        Assert.Contains("no coinciden", ex.Message);
        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_NoCumpleEdadMinima_LanzaExcepcionYNoRegistra()
    {
        _renaper.Respuesta = new ResultadoValidacionPersona(DatosCoinciden: true, CumpleEdadMinima: false);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez",
                CondicionIva.Monotributista));

        Assert.Contains("edad mínima", ex.Message);
        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_RenaperNoDisponible_PropagaExcepcionYNoRegistra()
    {
        _renaper.Error = new RenaperNoDisponibleException("RENAPER no tiene saldo de consultas.");

        await Assert.ThrowsAsync<RenaperNoDisponibleException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez",
                CondicionIva.Monotributista));

        Assert.Empty(_repositorio.Contribuyentes);
    }

    [Theory]
    [InlineData("30712345671")] // CUIT de persona jurídica
    [InlineData("20401112229")] // dígito verificador incorrecto
    [InlineData("2040111222")]  // le falta un dígito
    public async Task RegistrarPersonaHumana_CuitNoValidoParaPersonaHumana_LanzaSinConsultarRenaper(string cuit)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarPersonaHumanaAsync(cuit, "Lucas Matias", "Gomez", CondicionIva.Monotributista));

        Assert.Empty(_renaper.Consultas);
    }

    [Theory]
    [InlineData("", "Gomez")]
    [InlineData("Lucas Matias", " ")]
    public async Task RegistrarPersonaHumana_SinNombreOApellido_LanzaSinConsultarRenaper(string nombre,
        string apellido)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", nombre, apellido, CondicionIva.Monotributista));

        Assert.Empty(_renaper.Consultas);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_CondicionIvaInvalida_LanzaSinConsultarRenaper()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez", (CondicionIva)999));

        Assert.Empty(_renaper.Consultas);
    }

    [Fact]
    public async Task RegistrarPersonaHumana_CuitExistente_LanzaSinConsultarRenaper()
    {
        await _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez",
            CondicionIva.Monotributista);
        _renaper.Consultas.Clear();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarPersonaHumanaAsync("20401112228", "Lucas Matias", "Gomez",
                CondicionIva.Monotributista));

        Assert.Empty(_renaper.Consultas);
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
