using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Text;

public class PuntoVentaServicioTests
{
    private readonly RepositorioPuntoVentaFalso _puntoVentaRepositorio;
    private readonly RepositorioContribuyenteFalso _contribuyenteRepositorio;
    private readonly PuntoVentaServicio _servicio;

    public PuntoVentaServicioTests()
    {
        _puntoVentaRepositorio = new RepositorioPuntoVentaFalso();
        _contribuyenteRepositorio = new RepositorioContribuyenteFalso();
        _servicio = new PuntoVentaServicio(_puntoVentaRepositorio, _contribuyenteRepositorio);
    }

    [Fact]
    public void Registrar_PuntoVentaValido_RegistraCorrectamente()
    {
        var contribuyente = CrearContribuyente();
        _contribuyenteRepositorio.Agregar(contribuyente);
        var puntoVenta = CrearPuntoVenta();
        _servicio.Registrar(puntoVenta);
        Assert.Contains(puntoVenta, _puntoVentaRepositorio.PuntosVenta);
        Assert.Contains(puntoVenta, contribuyente.PuntosVenta);
        Assert.Same(contribuyente, puntoVenta.Contribuyente);
    }

    [Fact]
    public void Registrar_PuntoVentaNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => _servicio.Registrar(null!));
    }

    [Fact]
    public void Registrar_CUITInvalido_LanzaExcepcion()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.CUITContribuyente = "12345678901";
        Assert.Throws<ArgumentException>(() => _servicio.Registrar(puntoVenta));
    }

    [Fact]
    public void Registrar_NumeroNoPositivo_LanzaExcepcion()
    {
        var puntoVenta = CrearPuntoVenta();
        puntoVenta.Numero = 0;
        Assert.Throws<ArgumentException>(() => _servicio.Registrar(puntoVenta));
    }

    [Fact]
    public void Registrar_ContribuyenteInexistente_LanzaExcepcion()
    {
        var puntoVenta = CrearPuntoVenta();
        Assert.Throws<InvalidOperationException>(() => _servicio.Registrar(puntoVenta));
    }

    [Fact]
    public void Registrar_PuntoVentaDuplicado_LanzaExcepcion()
    {
        var contribuyente = CrearContribuyente();
        _contribuyenteRepositorio.Agregar(contribuyente);
        var existente = CrearPuntoVenta();
        _puntoVentaRepositorio.Agregar(existente);
        var duplicado = CrearPuntoVenta();
        Assert.Throws<InvalidOperationException>(() => _servicio.Registrar(duplicado));
    }

    [Fact]
    public void Registrar_ContribuyenteInexistente_NoRegistraPuntoVenta()
    {
        var puntoVenta = CrearPuntoVenta();
        Assert.Throws<InvalidOperationException>(() => _servicio.Registrar(puntoVenta));
        Assert.Empty(_puntoVentaRepositorio.PuntosVenta);
    }

    [Fact]
    public void ConsultarPorNumero_PuntoVentaExistente_DevuelvePuntoVenta()
    {
        var contribuyente = CrearContribuyente();
        _contribuyenteRepositorio.Agregar(contribuyente);
        var puntoVenta = CrearPuntoVenta();
        _servicio.Registrar(puntoVenta);
        var resultado = _servicio.ConsultarPorNumero(puntoVenta.CUITContribuyente, puntoVenta.Numero);
        Assert.Same(puntoVenta, resultado);
    }

    [Fact]
    public void ConsultarPorNumero_PuntoVentaInexistente_DevuelveNull()
    {
        var resultado = _servicio.ConsultarPorNumero("20301234563", 99);
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

    private static PuntoVenta CrearPuntoVenta()
    {
        return new PuntoVenta
        {
            CUITContribuyente = "20301234563",
            Numero = 1
        };
    }
}

internal class RepositorioPuntoVentaFalso : IPuntoVentaRepositorio
{
    public List<PuntoVenta> PuntosVenta { get; } = new();

    public PuntoVenta? ObtenerPorNumero(string cuitContribuyente, int numero)
    {
        return PuntosVenta.FirstOrDefault(p =>
            p.CUITContribuyente == cuitContribuyente &&
            p.Numero == numero);
    }

    public void Agregar(PuntoVenta puntoVenta)
    {
        PuntosVenta.Add(puntoVenta);
    }

    public void Actualizar(PuntoVenta puntoVenta)
    {
        int indice = PuntosVenta.FindIndex(p =>
            p.CUITContribuyente == puntoVenta.CUITContribuyente &&
            p.Numero == puntoVenta.Numero);

        if (indice >= 0)
            PuntosVenta[indice] = puntoVenta;
    }
}