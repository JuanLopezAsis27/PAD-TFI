using ArcaSim.Infraestructura.Integraciones.Renaper;

namespace ArcaSim.Tests.Infraestructura;

public class RenaperClienteSimuladoTests
{
    private sealed class RelojFijo(DateOnly hoy) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(hoy.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static RenaperClienteSimulado Crear(DateOnly? hoy = null) =>
        new(new RelojFijo(hoy ?? new DateOnly(2026, 11, 9)));

    [Fact]
    public async Task PersonaConocida_DatosCorrectos_EsValida()
    {
        var resultado = await Crear().ValidarPersonaAsync(40111222, "Lucas Matias", "Gomez", 18);

        Assert.True(resultado.DatosCoinciden);
        Assert.True(resultado.CumpleEdadMinima);
        Assert.True(resultado.EsValida);
    }

    [Theory]
    [InlineData("MARIA", "ejemplo")]
    [InlineData("María", "Ejemplo")]
    [InlineData("  maría ", " EJEMPLO  ")]
    public async Task ComparaSinDistinguirMayusculasTildesNiEspacios(string nombre, string apellido)
    {
        var resultado = await Crear().ValidarPersonaAsync(28765431, nombre, apellido, 18);

        Assert.True(resultado.DatosCoinciden);
    }

    [Fact]
    public async Task VariosEspaciosEntreNombres_Coincide()
    {
        var resultado = await Crear().ValidarPersonaAsync(40111222, "Lucas   Matias", "Gomez", 18);

        Assert.True(resultado.DatosCoinciden);
    }

    [Fact]
    public async Task NombreDistinto_NoCoincide()
    {
        var resultado = await Crear().ValidarPersonaAsync(40111222, "Lucas", "Gomez", 18);

        Assert.False(resultado.DatosCoinciden);
        Assert.True(resultado.CumpleEdadMinima);
        Assert.False(resultado.EsValida);
    }

    [Fact]
    public async Task DniInexistente_NoCoincide()
    {
        var resultado = await Crear().ValidarPersonaAsync(99999999, "Nadie", "Nadie", 18);

        Assert.False(resultado.DatosCoinciden);
        Assert.False(resultado.EsValida);
    }

    [Fact]
    public async Task MenorDeEdad_NoCumpleLaEdadMinima()
    {
        var resultado = await Crear().ValidarPersonaAsync(50111222, "Sofía", "Menor", 18);

        Assert.True(resultado.DatosCoinciden);
        Assert.False(resultado.CumpleEdadMinima);
    }

    [Theory]
    [InlineData(2027, 3, 13, false)] // el día anterior a cumplir 30
    [InlineData(2027, 3, 14, true)]  // el día que cumple 30
    public async Task EdadSeCalculaHastaElDiaDelCumpleanios(int anio, int mes, int dia, bool cumple)
    {
        // Lucas Matias Gomez nació el 14/03/1997.
        var resultado = await Crear(new DateOnly(anio, mes, dia)).ValidarPersonaAsync(40111222, "Lucas Matias",
            "Gomez", 30);

        Assert.Equal(cumple, resultado.CumpleEdadMinima);
    }
}
