using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;

namespace ArcaSim.Tests.Aplicacion;

public class ClaveFiscalServicioTests
{
    private const string Cuit = "20401112228";
    private const string ClaveValida = "Factura2026";

    private static readonly DateTimeOffset Ahora = new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly RepositorioUsuarioFalso _usuarios = new();
    private readonly RenaperClienteFalso _renaper = new();
    private readonly HasheadorClaveFalso _hasheador = new();
    private readonly RelojFalso _reloj = new(Ahora);
    private readonly ClaveFiscalServicio _servicio;

    public ClaveFiscalServicioTests()
    {
        _servicio = new ClaveFiscalServicio(_usuarios, _renaper, _hasheador, _reloj);
    }

    // ===== Registro (obtener la Clave Fiscal) =====

    [Fact]
    public async Task Registrar_DatosValidados_CreaLaCuentaConLaClaveHasheada()
    {
        var usuario = await RegistrarAsync();

        Assert.Same(usuario, Assert.Single(_usuarios.Usuarios));
        Assert.Equal(Cuit, usuario.Cuit);
        Assert.Equal("Lucas Matias", usuario.Nombre);
        Assert.Equal("Gomez", usuario.Apellido);
        Assert.Equal("lucas@example.com", usuario.Email);
        Assert.Equal(Ahora.UtcDateTime, usuario.FechaAlta);
        Assert.NotEqual(ClaveValida, usuario.HashClave);
        Assert.True(_hasheador.Verificar(ClaveValida, usuario.HashClave));
    }

    [Fact]
    public async Task Registrar_ConsultaARenaperElDniDeLaCuitYLaEdadMinima()
    {
        await _servicio.RegistrarAsync(Cuit, "  Lucas Matias ", " Gomez", "lucas@example.com", ClaveValida);

        var consulta = Assert.Single(_renaper.Consultas);
        Assert.Equal(40111222, consulta.Dni);
        Assert.Equal("Lucas Matias", consulta.Nombre);
        Assert.Equal("Gomez", consulta.Apellido);
        Assert.Equal(ClaveFiscalServicio.EdadMinima, consulta.EdadMinima);
    }

    [Fact]
    public async Task Registrar_DniConCeroAdelante_ConsultaElNumeroSinCeros()
    {
        await _servicio.RegistrarAsync("20051234562", "Ana", "Gomez", "ana@example.com", ClaveValida);

        Assert.Equal(5123456, Assert.Single(_renaper.Consultas).Dni);
    }

    [Fact]
    public async Task Registrar_DatosNoCoinciden_LanzaIdentidadNoValidadaYNoCrea()
    {
        _renaper.Respuesta = new ResultadoValidacionPersona(DatosCoinciden: false, CumpleEdadMinima: true);

        var ex = await Assert.ThrowsAsync<IdentidadNoValidadaException>(() => RegistrarAsync());

        Assert.Contains("no coinciden", ex.Message);
        Assert.Empty(_usuarios.Usuarios);
    }

    [Fact]
    public async Task Registrar_NoCumpleEdadMinima_LanzaIdentidadNoValidadaYNoCrea()
    {
        _renaper.Respuesta = new ResultadoValidacionPersona(DatosCoinciden: true, CumpleEdadMinima: false);

        var ex = await Assert.ThrowsAsync<IdentidadNoValidadaException>(() => RegistrarAsync());

        Assert.Contains("18 años", ex.Message);
        Assert.Empty(_usuarios.Usuarios);
    }

    [Fact]
    public async Task Registrar_RenaperNoDisponible_PropagaExcepcionYNoCrea()
    {
        _renaper.Error = new RenaperNoDisponibleException("RENAPER no tiene saldo de consultas.");

        await Assert.ThrowsAsync<RenaperNoDisponibleException>(() => RegistrarAsync());

        Assert.Empty(_usuarios.Usuarios);
    }

    [Theory]
    [InlineData("30712345671")] // CUIT de persona jurídica: no tiene Clave Fiscal propia
    [InlineData("20401112229")] // dígito verificador incorrecto
    [InlineData("2040111222")]  // le falta un dígito
    [InlineData("")]
    public async Task Registrar_CuitNoValidaParaPersonaHumana_LanzaSinConsultarRenaper(string cuit)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarAsync(cuit, "Lucas Matias", "Gomez", "lucas@example.com", ClaveValida));

        Assert.Empty(_renaper.Consultas);
    }

    [Fact]
    public async Task Registrar_DigitoVerificadorIncorrecto_DiceCualCorrespondeSinConsultarRenaper()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarAsync("20401112229", "Lucas Matias", "Gomez", "lucas@example.com", ClaveValida));

        Assert.Contains("20-40111222-8", ex.Message);
        Assert.Empty(_renaper.Consultas);
    }

    [Theory]
    [InlineData("", "Gomez")]
    [InlineData("Lucas Matias", " ")]
    public async Task Registrar_SinNombreOApellido_LanzaSinConsultarRenaper(string nombre, string apellido)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarAsync(Cuit, nombre, apellido, "lucas@example.com", ClaveValida));

        Assert.Empty(_renaper.Consultas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("lucas")]
    [InlineData("lucas@")]
    [InlineData("lucas@example")]
    [InlineData("Lucas Gomez <lucas@example.com>")]
    public async Task Registrar_EmailInvalido_LanzaSinConsultarRenaper(string email)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarAsync(Cuit, "Lucas Matias", "Gomez", email, ClaveValida));

        Assert.Empty(_renaper.Consultas);
    }

    [Theory]
    [InlineData("Corta12")]
    [InlineData("Facturacion")]   // sin números
    [InlineData("factura2026")]   // sin mayúscula
    [InlineData("Gomez40111222")] // contiene el DNI
    public async Task Registrar_ClaveQueNoCumpleLosRequisitos_LanzaSinConsultarRenaper(string clave)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarAsync(Cuit, "Lucas Matias", "Gomez", "lucas@example.com", clave));

        Assert.Contains("requisitos", ex.Message);
        Assert.Empty(_renaper.Consultas);
    }

    [Fact]
    public async Task Registrar_CuentaExistente_LanzaSinConsultarRenaper()
    {
        await RegistrarAsync();
        _renaper.Consultas.Clear();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RegistrarAsync());

        Assert.IsNotType<IdentidadNoValidadaException>(ex);
        Assert.Empty(_renaper.Consultas);
        Assert.Single(_usuarios.Usuarios);
    }

    // ===== Ingreso =====

    [Fact]
    public void IniciarSesion_ClaveCorrecta_IngresaYRegistraElIngreso()
    {
        var usuario = CrearCuenta();

        var resultado = _servicio.IniciarSesion(Cuit, ClaveValida);

        Assert.Equal(EstadoInicioSesion.Exitoso, resultado.Estado);
        Assert.Same(usuario, resultado.Usuario);
        Assert.Equal(Ahora.UtcDateTime, usuario.UltimoIngreso);
        Assert.Equal(1, _usuarios.Actualizaciones);
    }

    [Fact]
    public void IniciarSesion_CuitConEspacios_Ingresa()
    {
        CrearCuenta();

        Assert.Equal(EstadoInicioSesion.Exitoso, _servicio.IniciarSesion($" {Cuit} ", ClaveValida).Estado);
    }

    [Fact]
    public void IniciarSesion_ClaveIncorrecta_RechazaYCuentaElIntento()
    {
        var usuario = CrearCuenta();

        var resultado = _servicio.IniciarSesion(Cuit, "OtraClave2026");

        Assert.Equal(EstadoInicioSesion.CredencialesInvalidas, resultado.Estado);
        Assert.Null(resultado.Usuario);
        Assert.Equal(1, usuario.IntentosFallidos);
        Assert.Null(usuario.UltimoIngreso);
    }

    [Theory]
    [InlineData("20301234563")] // sin cuenta
    [InlineData("")]
    public void IniciarSesion_CuitSinCuenta_RechazaIgualQueUnaClaveIncorrecta(string cuit)
    {
        CrearCuenta();

        var resultado = _servicio.IniciarSesion(cuit, ClaveValida);

        Assert.Equal(EstadoInicioSesion.CredencialesInvalidas, resultado.Estado);
    }

    [Fact]
    public void IniciarSesion_CincoClavesIncorrectas_BloqueaLaClave()
    {
        var usuario = CrearCuenta();

        for (int i = 1; i < ClaveFiscalServicio.IntentosAntesDeBloquear; i++)
            Assert.Equal(EstadoInicioSesion.CredencialesInvalidas, _servicio.IniciarSesion(Cuit, "Incorrecta2026").Estado);

        var resultado = _servicio.IniciarSesion(Cuit, "Incorrecta2026");

        var hasta = (Ahora + ClaveFiscalServicio.DuracionBloqueo).UtcDateTime;
        Assert.Equal(EstadoInicioSesion.Bloqueado, resultado.Estado);
        Assert.Equal(hasta, resultado.BloqueadoHasta);
        Assert.Equal(hasta, usuario.BloqueadoHasta);
    }

    [Fact]
    public void IniciarSesion_ClaveBloqueada_RechazaAunqueLaClaveSeaCorrecta()
    {
        CrearCuenta();
        Bloquear();
        _reloj.Ahora = Ahora.AddMinutes(14);

        var resultado = _servicio.IniciarSesion(Cuit, ClaveValida);

        Assert.Equal(EstadoInicioSesion.Bloqueado, resultado.Estado);
    }

    [Fact]
    public void IniciarSesion_PasadoElBloqueo_PermiteIngresar()
    {
        var usuario = CrearCuenta();
        Bloquear();
        _reloj.Ahora = Ahora + ClaveFiscalServicio.DuracionBloqueo;

        var resultado = _servicio.IniciarSesion(Cuit, ClaveValida);

        Assert.Equal(EstadoInicioSesion.Exitoso, resultado.Estado);
        Assert.Null(usuario.BloqueadoHasta);
    }

    [Fact]
    public void IniciarSesion_IngresoCorrecto_ReiniciaLosIntentosFallidos()
    {
        var usuario = CrearCuenta();
        for (int i = 1; i < ClaveFiscalServicio.IntentosAntesDeBloquear; i++)
            _servicio.IniciarSesion(Cuit, "Incorrecta2026");

        _servicio.IniciarSesion(Cuit, ClaveValida);
        var resultado = _servicio.IniciarSesion(Cuit, "Incorrecta2026");

        Assert.Equal(EstadoInicioSesion.CredencialesInvalidas, resultado.Estado);
        Assert.Equal(1, usuario.IntentosFallidos);
    }

    private Task<Usuario> RegistrarAsync() =>
        _servicio.RegistrarAsync(Cuit, "Lucas Matias", "Gomez", "lucas@example.com", ClaveValida);

    private Usuario CrearCuenta()
    {
        var usuario = new Usuario
        {
            Cuit = Cuit,
            Nombre = "Lucas Matias",
            Apellido = "Gomez",
            Email = "lucas@example.com",
            HashClave = _hasheador.Hashear(ClaveValida),
            FechaAlta = Ahora.UtcDateTime
        };

        _usuarios.Agregar(usuario);
        return usuario;
    }

    private void Bloquear()
    {
        for (int i = 0; i < ClaveFiscalServicio.IntentosAntesDeBloquear; i++)
            _servicio.IniciarSesion(Cuit, "Incorrecta2026");
    }
}
