using ArcaSim.Contratos.Requests;
using ArcaSim.Contratos.Responses;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Api.Controllers;

/// <summary>
/// Funciones auxiliares para mapear las entidades del dominio a los DTOs JSON de la API REST.
/// </summary>
internal static class MapeadorRest
{
    public static ContribuyenteResponse MapearContribuyente(Contribuyente c) => new()
    {
        Cuit = c.CUIT,
        NombreORazonSocial = c.NombreORazonSocial,
        TipoPersona = c.TipoPersona.ToString(),
        CondicionIva = (int)c.CondicionIva,
        CondicionIvaDescripcion = DescribirCondicionIva(c.CondicionIva),
        PuntosVenta = c.PuntosVenta
            .OrderBy(pv => pv.Numero)
            .Select(MapearPuntoVenta)
            .ToList()
    };

    public static PuntoVentaResponse MapearPuntoVenta(PuntoVenta pv) => new()
    {
        CuitContribuyente = pv.CUITContribuyente,
        Numero = pv.Numero,
        Habilitado = pv.Habilitado
    };

    public static ComprobanteResponse MapearComprobante(Comprobante c)
    {
        string? urlQr = null;
        if (c.EstadoAutorizacion is EstadoAutorizacionComprobante.Autorizado
            or EstadoAutorizacionComprobante.AutorizadoConObservaciones)
        {
            try
            {
                urlQr = ServicioQrComprobante.GenerarUrl(c);
            }
            catch
            {
                urlQr = null;
            }
        }

        return new ComprobanteResponse
        {
            CuitEmisor = c.CUITEmisor,
            PuntoVentaNumero = c.PuntoVentaNumero,
            TipoComprobante = (int)c.Tipo,
            TipoComprobanteDescripcion = DescribirTipoComprobante(c.Tipo),
            Numero = c.Numero,
            Concepto = (int)c.Concepto,
            Fecha = c.Fecha,
            FechaServicioDesde = c.FechaServicioDesde,
            FechaServicioHasta = c.FechaServicioHasta,
            FechaVencimientoPago = c.FechaVencimientoPago,
            TipoDocumentoReceptor = (int)c.TipoDocumentoReceptor,
            NumeroDocumentoReceptor = c.NumeroDocumentoReceptor,
            CondicionIvaReceptor = c.CondicionIvaReceptor.HasValue ? (int)c.CondicionIvaReceptor.Value : null,
            ImporteNeto = c.ImporteNeto,
            ImporteNoGravado = c.ImporteNoGravado,
            ImporteExento = c.ImporteExento,
            ImporteTributos = c.ImporteTributos,
            ImporteIva = c.ImporteIVA,
            ImporteTotal = c.ImporteTotal,
            Moneda = c.Moneda,
            Cotizacion = c.Cotizacion,
            CancelaEnMismaMonedaExtranjera = c.CancelaEnMismaMonedaExtranjera,
            Cae = c.CAE,
            FechaVencimientoCae = c.FechaVencimientoCAE,
            EstadoAutorizacion = c.EstadoAutorizacion.ToString(),
            Observaciones = c.ObservacionesAutorizacion.ToList(),
            UrlQr = urlQr,
            AlicuotasIva = c.AlicuotasIva.Select(a => new AlicuotaIvaDto
            {
                AlicuotaId = (int)a.Alicuota,
                BaseImponible = a.BaseImponible,
                ImporteIva = a.ImporteIVA
            }).ToList()
        };
    }

    public static string DescribirCondicionIva(CondicionIva condicion) => condicion switch
    {
        CondicionIva.ResponsableInscripto => "IVA Responsable Inscripto",
        CondicionIva.Exento => "IVA Sujeto Exento",
        CondicionIva.ConsumidorFinal => "Consumidor Final",
        CondicionIva.Monotributista => "Responsable Monotributo",
        CondicionIva.SujetoNoCategorizado => "Sujeto No Categorizado",
        CondicionIva.ProveedorDelExterior => "Proveedor del Exterior",
        CondicionIva.ClienteDelExterior => "Cliente del Exterior",
        CondicionIva.IvaLiberadoLey19640 => "IVA Liberado – Ley N° 19.640",
        CondicionIva.MonotributistaSocial => "Monotributista Social",
        CondicionIva.IvaNoAlcanzado => "IVA No Alcanzado",
        CondicionIva.MonotributoTrabajadorIndependientePromovido => "Monotributo Trabajador Independiente Promovido",
        _ => condicion.ToString()
    };

    public static string DescribirTipoComprobante(TipoComprobante tipo) => tipo switch
    {
        TipoComprobante.FacturaA => "Factura A",
        TipoComprobante.NotaDebitoA => "Nota de Débito A",
        TipoComprobante.NotaCreditoA => "Nota de Crédito A",
        TipoComprobante.FacturaB => "Factura B",
        TipoComprobante.NotaDebitoB => "Nota de Débito B",
        TipoComprobante.NotaCreditoB => "Nota de Crédito B",
        TipoComprobante.FacturaC => "Factura C",
        TipoComprobante.NotaDebitoC => "Nota de Débito C",
        TipoComprobante.NotaCreditoC => "Nota de Crédito C",
        _ => tipo.ToString()
    };
}

