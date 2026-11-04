using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

public class ContribuyenteServicioTests
{
    private readonly RepositorioContribuyenteFalso _repositorio;
    private readonly ContribuyenteServicio _servicio;

    public ContribuyenteServicioTests()
    {
        _repositorio = new RepositorioContribuyenteFalso();
        _servicio = new ContribuyenteServicio(_repositorio);
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

    private static Contribuyente CrearContribuyente()
    {
        return new Contribuyente
        {
            CUIT = "20301234563",
            NombreORazonSocial = "Empresa de prueba",
            TipoPersona = Enum.GetValues<TipoPersona>()[0],
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