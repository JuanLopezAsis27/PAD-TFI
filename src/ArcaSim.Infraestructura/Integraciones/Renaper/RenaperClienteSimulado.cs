using System.Globalization;
using System.Text;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;

namespace ArcaSim.Infraestructura.Integraciones.Renaper;

/// <summary>
/// RENAPER simulado para desarrollo: responde igual que el servicio real, pero con personas ficticias en
/// memoria, sin red y sin consumir créditos. Compara nombre y apellido sin distinguir mayúsculas ni tildes.
/// </summary>
public sealed class RenaperClienteSimulado(TimeProvider reloj) : IRenaperCliente
{
    public sealed record PersonaSimulada(int Dni, string Nombre, string Apellido, DateOnly FechaNacimiento);

    // La lista y los CUIT que corresponden a cada DNI están en docs/renaper.md.
    public static readonly IReadOnlyList<PersonaSimulada> Personas =
    [
        new(40111222, "Lucas Matias", "Gomez", new DateOnly(1997, 3, 14)),
        new(28765431, "María", "Ejemplo", new DateOnly(1981, 6, 2)),
        new(30123456, "Juan Carlos", "Pérez", new DateOnly(1983, 9, 21)),
        new(35987654, "Ana Laura", "Fernández", new DateOnly(1991, 11, 5)),
        new(50111222, "Sofía", "Menor", new DateOnly(2012, 5, 10))
    ];

    public RenaperClienteSimulado() : this(TimeProvider.System)
    {
    }

    public Task<ResultadoValidacionPersona> ValidarPersonaAsync(int dni, string nombre, string apellido,
        int edadMinima, CancellationToken cancellationToken = default)
    {
        var persona = Personas.FirstOrDefault(p => p.Dni == dni);

        // Como el servicio real: un DNI inexistente se informa como "datos no coinciden".
        bool datosCoinciden = persona is not null &&
            Normalizar(persona.Nombre) == Normalizar(nombre) &&
            Normalizar(persona.Apellido) == Normalizar(apellido);

        bool cumpleEdad = persona is not null && Edad(persona.FechaNacimiento) >= edadMinima;

        return Task.FromResult(new ResultadoValidacionPersona(datosCoinciden, cumpleEdad));
    }

    private int Edad(DateOnly nacimiento)
    {
        var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
        int edad = hoy.Year - nacimiento.Year;
        return nacimiento > hoy.AddYears(-edad) ? edad - 1 : edad;
    }

    private static string Normalizar(string texto)
    {
        var sinTildes = new StringBuilder();

        foreach (char c in texto.Trim().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sinTildes.Append(c);
        }

        return string.Join(' ', sinTildes.ToString().ToUpperInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
