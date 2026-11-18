using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ArcaSim.Dominio.Reglas;

/// <summary>
/// Arma la URL del código QR de un comprobante según la especificación de ARCA: los datos van en el
/// parámetro <c>p</c> (JSON en Base64). La página que los verifica la decide quien llama.
/// </summary>
public static class GeneradorQrComprobante
{
    /// <summary>Página de verificación de ARCA real, la que indica su especificación.</summary>
    public const string UrlBaseArca = "https://www.arca.gob.ar/fe/qr/";

    /// <param name="datos">Datos del comprobante autorizado.</param>
    /// <param name="urlBase">Página que abre el QR al escanearlo, por ejemplo
    /// <c>https://portal/fe/qr/</c>. Absoluta (http o https) y sin parámetros.</param>
    public static string GenerarUrl(DatosQrComprobante datos, string urlBase)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarUrlBase(urlBase);
        ValidarDatos(datos);

        var contenido = new Dictionary<string, object?>
        {
            ["ver"] = 1,
            ["fecha"] = datos.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["cuit"] = long.Parse(datos.CUITEmisor, CultureInfo.InvariantCulture),
            ["ptoVta"] = datos.PuntoVenta,
            ["tipoCmp"] = datos.TipoComprobante,
            ["nroCmp"] = datos.NumeroComprobante,
            ["importe"] = datos.ImporteTotal,
            ["moneda"] = datos.Moneda,
            ["ctz"] = datos.Cotizacion
        };
        if (datos.TipoDocumentoReceptor.HasValue)
        {
            contenido["tipoDocRec"] = datos.TipoDocumentoReceptor.Value;
            contenido["nroDocRec"] = datos.NumeroDocumentoReceptor!;
        }
        contenido["tipoCodAut"] = datos.TipoAutorizacion;
        contenido["codAut"] = long.Parse(datos.CodigoAutorizacion, CultureInfo.InvariantCulture);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var elemento in contenido)
            {
                writer.WritePropertyName(elemento.Key);
                if (elemento.Key == "nroDocRec")
                {
                    writer.WriteRawValue(datos.NumeroDocumentoReceptor!);
                }
                else
                {
                    JsonSerializer.Serialize(
                        writer,
                        elemento.Value,
                        elemento.Value?.GetType() ?? typeof(object));
                }
            }
            writer.WriteEndObject();
        }
        string json = Encoding.UTF8.GetString(stream.ToArray());
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        string base64Url = Uri.EscapeDataString(base64);

        return $"{urlBase}?p={base64Url}";
    }

    /// <summary>Indica si <paramref name="urlBase"/> sirve como página de verificación del QR.</summary>
    public static bool EsUrlBaseValida(string? urlBase)
    {
        return Uri.TryCreate(urlBase, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);
    }

    private static void ValidarUrlBase(string urlBase)
    {
        if (!EsUrlBaseValida(urlBase))
        {
            throw new ArgumentException(
                "La URL base del QR debe ser absoluta (http o https) y no tener parámetros.", nameof(urlBase));
        }
    }

    private static void ValidarDatos(DatosQrComprobante datos)
    {
        if (datos.Fecha == default)
            throw new ArgumentException("La fecha es obligatoria.");

        if (!ValidadorCUIT.EsValido(datos.CUITEmisor))
            throw new ArgumentException("El CUIT del emisor no es válido.");

        if (datos.PuntoVenta <= 0 || datos.PuntoVenta > 99999)
            throw new ArgumentException("El punto de venta debe estar entre 1 y 99999.");

        if (!Enum.IsDefined(typeof(TipoComprobante), datos.TipoComprobante) || datos.TipoComprobante > 999)
            throw new ArgumentException("El tipo de comprobante no es válido.");

        if (datos.NumeroComprobante <= 0 || datos.NumeroComprobante > 99999999)
            throw new ArgumentException("El número de comprobante debe estar entre 1 y 99999999.");

        if (datos.ImporteTotal < 0 ||decimal.Round(datos.ImporteTotal, 2) != datos.ImporteTotal
            || datos.ImporteTotal >= 10000000000000m)
            throw new ArgumentException("El importe debe ser positivo o cero y tener hasta 13 dígitos" +
                " enteros, con 2 o menos decimales.");

        if (!ValidadorMoneda.EsValida(datos.Moneda))
            throw new ArgumentException("La moneda no es válida.");

        if (datos.Cotizacion <= 0 ||decimal.Round(datos.Cotizacion, 6) != datos.Cotizacion
            || datos.Cotizacion >= 10000000000000m)
            throw new ArgumentException("La cotización debe ser positiva y tener hasta 13 dígitos enteros," +
                "con 6 o menos decimales.");

        bool tieneTipoDocumento = datos.TipoDocumentoReceptor.HasValue;
        bool tieneNumeroDocumento = !string.IsNullOrWhiteSpace(datos.NumeroDocumentoReceptor);

        if (tieneTipoDocumento != tieneNumeroDocumento)
            throw new ArgumentException("El tipo y número de documento del receptor deben informarse juntos.");

        if (tieneNumeroDocumento)
        {
            if (!Enum.IsDefined(typeof(TipoDocumento), datos.TipoDocumentoReceptor!.Value))
            {
                throw new ArgumentException("El tipo de documento del receptor no es válido.");
            }

            if (datos.NumeroDocumentoReceptor!.Length > 20 ||
                !datos.NumeroDocumentoReceptor.All(char.IsAsciiDigit))
            {
                throw new ArgumentException("El número de documento debe contener hasta 20 dígitos.");
            }
        }

        if (datos.TipoAutorizacion is not ("E" or "A"))
            throw new ArgumentException("El tipo de autorización debe ser E o A.");

        if (string.IsNullOrWhiteSpace(datos.CodigoAutorizacion) || datos.CodigoAutorizacion.Length != 14
            || !datos.CodigoAutorizacion.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("El código de autorización debe tener 14 dígitos.");
        }
    }
}