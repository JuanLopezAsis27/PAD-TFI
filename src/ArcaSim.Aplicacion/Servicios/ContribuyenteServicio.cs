using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ArcaSim.Aplicacion.Excepciones;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Aplicacion.Servicios;

public class ContribuyenteServicio
{
    /// <summary>Edad mínima para que una persona humana se inscriba como contribuyente.</summary>
    public const int EdadMinimaPersonaHumana = 18;

    private static readonly string[] PrefijosPersonaHumana = ["20", "23", "24", "27"];

    private readonly IContribuyenteRepositorio _repositorio;
    private readonly IRenaperCliente _renaper;

    public ContribuyenteServicio(IContribuyenteRepositorio repositorio, IRenaperCliente renaper)
    {
        _repositorio = repositorio;
        _renaper = renaper;
    }

    /// <summary>
    /// Alta de una persona jurídica. Las personas humanas se registran con
    /// <see cref="RegistrarPersonaHumanaAsync"/>, que valida su identidad con RENAPER.
    /// </summary>
    public void Registrar(Contribuyente contribuyente)
    {
        ArgumentNullException.ThrowIfNull(contribuyente);

        if (contribuyente.TipoPersona == TipoPersona.Humana)
        {
            throw new InvalidOperationException(
                "Las personas humanas se registran con RegistrarPersonaHumanaAsync, que valida su identidad con RENAPER.");
        }

        ValidarYAgregar(contribuyente);
    }

    /// <summary>
    /// Alta de una persona humana. Su CUIT es prefijo + DNI + dígito verificador: se extrae el DNI y se
    /// valida con RENAPER que el nombre y el apellido coincidan y que cumpla la edad mínima.
    /// </summary>
    /// <exception cref="ArgumentException">Datos inválidos (no se consulta a RENAPER).</exception>
    /// <exception cref="InvalidOperationException">El CUIT ya existe o RENAPER no validó a la persona.</exception>
    /// <exception cref="RenaperNoDisponibleException">RENAPER no dio una respuesta válida.</exception>
    public async Task<Contribuyente> RegistrarPersonaHumanaAsync(string cuit, string nombre, string apellido,
        CondicionIva condicionIva, CancellationToken cancellationToken = default)
    {
        if (!ValidadorCUIT.EsValido(cuit))
            throw new ArgumentException("El CUIT del contribuyente no es válido.");

        if (!PrefijosPersonaHumana.Contains(cuit[..2]))
            throw new ArgumentException("El CUIT no es de una persona humana: debe empezar con 20, 23, 24 o 27.");

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El nombre y el apellido son obligatorios.");

        if (!Enum.IsDefined(condicionIva))
            throw new ArgumentException("La condición de IVA no es válida.");

        // Se controla antes de consultar a RENAPER, porque cada consulta consume un crédito.
        if (_repositorio.ObtenerPorCUIT(cuit) is not null)
            throw new InvalidOperationException("Ya existe un contribuyente registrado con ese CUIT.");

        int dni = int.Parse(cuit.AsSpan(2, 8), CultureInfo.InvariantCulture);
        nombre = nombre.Trim();
        apellido = apellido.Trim();

        var resultado = await _renaper.ValidarPersonaAsync(dni, nombre, apellido, EdadMinimaPersonaHumana,
            cancellationToken);

        if (!resultado.DatosCoinciden)
        {
            throw new InvalidOperationException(
                $"El nombre y el apellido no coinciden con los que RENAPER tiene para el DNI {dni}.");
        }

        if (!resultado.CumpleEdadMinima)
        {
            throw new InvalidOperationException(
                $"La persona no cumple la edad mínima de {EdadMinimaPersonaHumana} años para inscribirse.");
        }

        var contribuyente = new Contribuyente
        {
            CUIT = cuit,
            NombreORazonSocial = $"{apellido}, {nombre}",
            TipoPersona = TipoPersona.Humana,
            CondicionIva = condicionIva
        };

        ValidarYAgregar(contribuyente);
        return contribuyente;
    }

    public Contribuyente? ConsultarPorCUIT(string cuit)
    {
        return _repositorio.ObtenerPorCUIT(cuit);
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
