using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ArcaSim.Aplicacion.Servicios;

public class ComprobanteServicio
{
    private readonly IContribuyenteRepositorio _contribuyenteRepositorio;
    private readonly IPuntoVentaRepositorio _puntoVentaRepositorio;
    private readonly IComprobanteRepositorio _comprobanteRepositorio;

    private readonly GeneradorCaeSimulado _generadorCae;
    private readonly OpcionesQr _qr;

    /// <param name="opcionesQr">Adónde lleva el QR de los comprobantes. Si no se indica, a ARCA real
    /// (alcanza para los datos de prueba y los tests, que no lo escanean).</param>
    public ComprobanteServicio(IContribuyenteRepositorio contribuyenteRepositorio,
        IPuntoVentaRepositorio puntoVentaRepositorio,
        IComprobanteRepositorio comprobanteRepositorio,
        OpcionesQr? opcionesQr = null)
    {
        _contribuyenteRepositorio = contribuyenteRepositorio;
        _puntoVentaRepositorio = puntoVentaRepositorio;
        _comprobanteRepositorio = comprobanteRepositorio;
        _generadorCae = new GeneradorCaeSimulado();
        _qr = opcionesQr ?? OpcionesQr.Arca;
    }

    public void AutorizarSimuladamente(string cuitEmisor, int puntoVentaNumero, TipoComprobante tipo,
        long numero)
    {
        var comprobante = _comprobanteRepositorio.ObtenerPorIdentificacion(cuitEmisor, puntoVentaNumero,
            tipo, numero);

        if (comprobante is null)
            throw new InvalidOperationException("No existe el comprobante indicado.");

        if (comprobante.EstadoAutorizacion is EstadoAutorizacionComprobante.Autorizado or
            EstadoAutorizacionComprobante.AutorizadoConObservaciones)
        {
            throw new InvalidOperationException("El comprobante ya se encuentra autorizado.");
        }

        var resultado = _generadorCae.Generar(DateOnly.FromDateTime(comprobante.Fecha));

        AplicarResultadoAutorizacion(cuitEmisor, puntoVentaNumero, tipo, numero, resultado);
    }

    public ResultadoValidacion Registrar(Comprobante comprobante, DateTime fechaGeneracion)
    {
        ArgumentNullException.ThrowIfNull(comprobante);

        var contribuyente = _contribuyenteRepositorio.ObtenerPorCUIT(comprobante.CUITEmisor);

        if (contribuyente is null)
        {
            return ResultadoValidacion.ConErrores(new ErrorValidacion("DOM-012",
                    "No existe el contribuyente emisor."));
        }

        var puntoVenta = _puntoVentaRepositorio.ObtenerPorNumero(comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero);

        if (puntoVenta is null)
        {
            return ResultadoValidacion.ConErrores(new ErrorValidacion("DOM-013",
                    "No existe el punto de venta indicado."));
        }

        // Un comprobante rechazado no ocupa su número (como en ARCA): se puede volver a enviar.
        var existente = _comprobanteRepositorio.ObtenerPorIdentificacion(comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero, comprobante.Tipo, comprobante.Numero);

        if (existente is not null &&
            existente.EstadoAutorizacion != EstadoAutorizacionComprobante.Rechazado)
        {
            return ResultadoValidacion.ConErrores(new ErrorValidacion("DOM-014",
                    "Ya existe un comprobante con esa identificación."));
        }

        comprobante.PuntoVenta = puntoVenta;

        var ultimoAutorizado = _comprobanteRepositorio.ObtenerUltimoAutorizado(comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero, comprobante.Tipo);

        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            contribuyente.CondicionIva, fechaGeneracion, ultimoAutorizado?.Numero ?? 0,
            ultimoAutorizado?.Fecha);

        if (!resultado.EsValido)
            return resultado;

        if (existente is not null)
        {
            puntoVenta.Comprobantes.Remove(existente);
            _comprobanteRepositorio.Eliminar(existente);
        }

        puntoVenta.Comprobantes.Add(comprobante);
        _comprobanteRepositorio.Agregar(comprobante);

        return ResultadoValidacion.Exito();
    }

    public void AplicarResultadoAutorizacion(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo, long numero, ResultadoAutorizacion resultado)
    {
        ArgumentNullException.ThrowIfNull(resultado);

        var comprobante = _comprobanteRepositorio.ObtenerPorIdentificacion(cuitEmisor, puntoVentaNumero,
            tipo, numero);

        if (comprobante is null)
            throw new InvalidOperationException("No existe el comprobante indicado.");

        comprobante.AplicarResultadoAutorizacion(resultado);

        _comprobanteRepositorio.Actualizar(comprobante);
    }

    public Comprobante? Consultar(string cuitEmisor, int puntoVentaNumero, TipoComprobante tipo,
        long numero)
    {
        return _comprobanteRepositorio.ObtenerPorIdentificacion(cuitEmisor, puntoVentaNumero,
            tipo, numero);
    }

    public long ConsultarUltimoNumeroAutorizado(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo)
    {
        return _comprobanteRepositorio.ObtenerUltimoAutorizado(cuitEmisor, puntoVentaNumero, tipo)?.Numero ?? 0;
    }

    /// <summary>Comprobantes emitidos por esos CUIT, del más reciente al más viejo.</summary>
    public IReadOnlyList<Comprobante> ListarPorEmisores(IReadOnlyCollection<string> cuitsEmisores)
    {
        ArgumentNullException.ThrowIfNull(cuitsEmisores);
        return cuitsEmisores.Count == 0 ? [] : _comprobanteRepositorio.ObtenerPorEmisores(cuitsEmisores);
    }

    /// <summary>
    /// URL del código QR del comprobante, que lleva a la constatación configurada en <see cref="OpcionesQr"/>.
    /// Devuelve null si no está autorizado o si le falta algún dato del QR (por ejemplo, la cotización de
    /// un comprobante en moneda extranjera que se cancela en esa moneda).
    /// </summary>
    public string? GenerarUrlQr(Comprobante comprobante)
    {
        ArgumentNullException.ThrowIfNull(comprobante);

        if (comprobante.EstadoAutorizacion is not (EstadoAutorizacionComprobante.Autorizado or
            EstadoAutorizacionComprobante.AutorizadoConObservaciones))
        {
            return null;
        }

        try
        {
            return ServicioQrComprobante.GenerarUrl(comprobante, _qr.UrlBase);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return null;
        }
    }

    public ResultadoConstatacion Constatar(string cuitEmisor, int puntoVentaNumero,
        TipoComprobante tipo, long numero, string cae)
    {
        var comprobante = _comprobanteRepositorio.ObtenerPorIdentificacion(cuitEmisor, puntoVentaNumero,
            tipo, numero);

        if (comprobante is null)
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "El comprobante no existe."
            };

        if (comprobante.EstadoAutorizacion is not (EstadoAutorizacionComprobante.Autorizado or
            EstadoAutorizacionComprobante.AutorizadoConObservaciones))
        {
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "El comprobante no se encuentra autorizado."
            };
        }

        if (string.IsNullOrWhiteSpace(cae) ||
            !string.Equals(comprobante.CAE, cae, StringComparison.Ordinal))
        {
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "El CAE informado no coincide con el registrado."
            };
        }

        return new ResultadoConstatacion
        {
            EsValido = true,
            Mensaje = "El comprobante es válido.",
            Comprobante = comprobante
        };
    }

    /// <summary>
    /// Constata un comprobante con la URL de su código QR. Se aceptan las del portal (las que genera
    /// ArcaSim) y las de ARCA real, porque un sistema que sigue la especificación al pie de la letra
    /// imprime esas.
    /// </summary>
    public ResultadoConstatacion ConstatarPorQr(string urlQr)
    {
        if (string.IsNullOrWhiteSpace(urlQr) || !Uri.TryCreate(urlQr, UriKind.Absolute, out var uri) ||
            !EsPaginaDeVerificacionQr(uri))
        {
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "La URL del código QR no es válida."
            };
        }

        var parametros = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2)).Where(p => p.Length == 2 && p[0] == "p").ToList();

        if (parametros.Count != 1 || string.IsNullOrWhiteSpace(parametros[0][1]))
        {
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "La URL del código QR no contiene los datos necesarios."
            };
        }

        try
        {
            var base64 = Uri.UnescapeDataString(parametros[0][1]);
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            using var documento = JsonDocument.Parse(json);
            var datos = documento.RootElement;

            var version = datos.GetProperty("ver").GetInt32();
            var fecha = DateOnly.ParseExact(datos.GetProperty("fecha").GetString()!, "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            var cuit = LeerCampoComoTexto(datos, "cuit");
            var puntoVenta = datos.GetProperty("ptoVta").GetInt32();
            var tipo = (TipoComprobante)datos.GetProperty("tipoCmp").GetInt32();
            var numero = datos.GetProperty("nroCmp").GetInt64();
            var importe = datos.GetProperty("importe").GetDecimal();
            var moneda = datos.GetProperty("moneda").GetString();
            var cotizacion = datos.GetProperty("ctz").GetDecimal();

            var tieneTipoDocumento = datos.TryGetProperty("tipoDocRec", out var tipoDocumento);
            var tieneNumeroDocumento = datos.TryGetProperty("nroDocRec", out var numeroDocumento);

            if (tieneTipoDocumento != tieneNumeroDocumento)
            {
                throw new JsonException();
            }

            int? tipoDocReceptor = tieneTipoDocumento
                ? tipoDocumento.GetInt32()
                : null;

            string? numeroDocReceptor = tieneNumeroDocumento
                ? numeroDocumento.GetRawText()
                : null;

            var tipoAutorizacion = datos.GetProperty("tipoCodAut").GetString();
            var cae = LeerCampoComoTexto(datos, "codAut");

            var resultado = Constatar(cuit, puntoVenta, tipo, numero, cae);

            if (!resultado.EsValido)
                return resultado;

            var comprobante = _comprobanteRepositorio.ObtenerPorIdentificacion(
                cuit, puntoVenta, tipo, numero);

            if (comprobante is null)
            {
                return new ResultadoConstatacion
                {
                    EsValido = false,
                    Mensaje = "El comprobante no existe."
                };
            }

            bool documentoCoincide =
                tipoDocReceptor == (comprobante.NumeroDocumentoReceptor == string.Empty
                    ? null
                    : (int?)comprobante.TipoDocumentoReceptor) &&
                numeroDocReceptor == (string.IsNullOrWhiteSpace(comprobante.NumeroDocumentoReceptor)
                    ? null
                    : comprobante.NumeroDocumentoReceptor);

            bool datosCoinciden =
                version == 1 &&
                fecha == DateOnly.FromDateTime(comprobante.Fecha) &&
                importe == comprobante.ImporteTotal &&
                moneda == comprobante.Moneda &&
                cotizacion == comprobante.Cotizacion &&
                tipoAutorizacion == "E" &&
                documentoCoincide;

            if (!datosCoinciden)
            {
                return new ResultadoConstatacion
                {
                    EsValido = false,
                    Mensaje = "Los datos del código QR no coinciden con el comprobante registrado."
                };
            }

            return resultado;
        }
        catch (Exception ex) when (ex is FormatException || ex is JsonException ||
            ex is KeyNotFoundException || ex is InvalidOperationException || ex is OverflowException ||
            ex is ArgumentException)
        {
            return new ResultadoConstatacion
            {
                EsValido = false,
                Mensaje = "No se pudieron interpretar los datos del código QR."
            };
        }
    }

    private bool EsPaginaDeVerificacionQr(Uri uri)
    {
        bool esArca = uri.Scheme == Uri.UriSchemeHttps
            && uri.Host is "www.arca.gob.ar" or "arca.gob.ar"
            && MismaRuta(uri.AbsolutePath, "/fe/qr");

        // Se comparan el host y la ruta, no el esquema ni el puerto: detrás de un proxy (como en Render) el
        // portal puede recibir la página por http aunque su dirección pública sea https.
        var portal = new Uri(_qr.UrlBase);
        bool esPortal = !_qr.ApuntaAArca
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && string.Equals(uri.Host, portal.Host, StringComparison.OrdinalIgnoreCase)
            && MismaRuta(uri.AbsolutePath, portal.AbsolutePath);

        return esArca || esPortal;
    }

    private static bool MismaRuta(string ruta, string esperada) =>
        string.Equals(ruta.TrimEnd('/'), esperada.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

    private static string LeerCampoComoTexto(JsonElement datos, string nombre)
    {
        var elemento = datos.GetProperty(nombre);
        return elemento.ValueKind switch
        {
            JsonValueKind.String => elemento.GetString()!,
            JsonValueKind.Number => elemento.GetRawText(),
            _ => throw new FormatException($"El campo {nombre} no es válido.")
        };
    }
}