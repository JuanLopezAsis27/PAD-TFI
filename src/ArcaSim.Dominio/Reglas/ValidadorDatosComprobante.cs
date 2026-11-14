using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorDatosComprobante
{
    public static bool EsValido(Comprobante? comprobante)
    {
        if (comprobante is null)
            return false;

        if (!ValidadorCUIT.EsValido(comprobante.CUITEmisor))
            return false;

        if (comprobante.PuntoVentaNumero <= 0 || comprobante.Numero <= 0 ||
            comprobante.Fecha == default)
            return false;

        if (!Enum.IsDefined(typeof(TipoComprobante), comprobante.Tipo)
            || !Enum.IsDefined(typeof(ConceptoComprobante), comprobante.Concepto))
            return false;

        if (!Enum.IsDefined(typeof(TipoDocumento), comprobante.TipoDocumentoReceptor))
            return false;

        if (!EsNumeroDocumentoValido(comprobante.TipoDocumentoReceptor,
            comprobante.NumeroDocumentoReceptor))
            return false;

        if (!ValidadorMoneda.EsValida(comprobante.Moneda))
            return false;

        if (comprobante.Moneda == "PES")
            return comprobante.Cotizacion == 1m;

        if (comprobante.Cotizacion is null)
        {
            return comprobante.CancelaEnMismaMonedaExtranjera && EsFactura(comprobante.Tipo);
        }

        if (comprobante.Cotizacion <= 0)
            return false;

        return true;
    }

    private static bool EsFactura(TipoComprobante tipo)
    {
        return tipo is TipoComprobante.FacturaA
            or TipoComprobante.FacturaB
            or TipoComprobante.FacturaC;
    }

    private static bool EsNumeroDocumentoValido(TipoDocumento tipoDocumento, string? numeroDocumento)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento) || !numeroDocumento.All(char.IsAsciiDigit))
            return false;

        return tipoDocumento switch
        {
            TipoDocumento.CUIT or TipoDocumento.CUIL => ValidadorCUIT.EsValido(numeroDocumento),
            TipoDocumento.DNI => numeroDocumento.Length is 7 or 8,
            TipoDocumento.SinIdentificar => numeroDocumento == "0",
            _ => false
        };
    }
}