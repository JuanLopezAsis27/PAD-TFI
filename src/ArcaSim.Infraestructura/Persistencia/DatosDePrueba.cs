using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace ArcaSim.Infraestructura.Persistencia;

/// <summary>
/// Contribuyentes, puntos de venta y comprobantes ficticios para probar el sistema.
/// Se cargan con los servicios de la aplicación, así pasan por las mismas validaciones que el resto
/// (salvo la validación de identidad con RENAPER de la persona humana).
/// Cada contribuyente se carga una sola vez: si ya existe, se saltea junto con sus datos.
/// </summary>
public static class DatosDePrueba
{
    public const string CuitResponsableInscripto = "30712345671";
    public const string CuitMonotributista = "27287654311";
    public const string CuitExento = "30709998885";

    /// <returns>La cantidad de contribuyentes nuevos que se cargaron.</returns>
    public static int Cargar(IServiceProvider services)
    {
        var contribuyentes = services.GetRequiredService<IContribuyenteRepositorio>();
        var puntosVenta = services.GetRequiredService<IPuntoVentaRepositorio>();
        var comprobantes = services.GetRequiredService<IComprobanteRepositorio>();

        var cargador = new Cargador(
            new ContribuyenteServicio(contribuyentes, services.GetRequiredService<IRenaperCliente>()),
            contribuyentes,
            new PuntoVentaServicio(puntosVenta, contribuyentes),
            new ComprobanteServicio(contribuyentes, puntosVenta, comprobantes),
            DateTime.Today);

        int cargados = 0;

        if (cargador.CargarSiNoExiste(
            new Contribuyente
            {
                CUIT = CuitResponsableInscripto,
                NombreORazonSocial = "Comercio Demo S.A.",
                TipoPersona = TipoPersona.Juridica,
                CondicionIva = CondicionIva.ResponsableInscripto
            },
            puntosVentaHabilitados: [1, 2],
            puntosVentaDeshabilitados: [3],
            hoy =>
            [
                FacturaA(hoy),
                FacturaBAConsumidorFinal(hoy)
            ]))
        {
            cargados++;
        }

        if (cargador.CargarSiNoExiste(
            new Contribuyente
            {
                CUIT = CuitMonotributista,
                NombreORazonSocial = "María Ejemplo",
                TipoPersona = TipoPersona.Humana,
                CondicionIva = CondicionIva.Monotributista
            },
            puntosVentaHabilitados: [1],
            puntosVentaDeshabilitados: [],
            hoy => [FacturaCDeServicios(hoy)]))
        {
            cargados++;
        }

        if (cargador.CargarSiNoExiste(
            new Contribuyente
            {
                CUIT = CuitExento,
                NombreORazonSocial = "Fundación Demo",
                TipoPersona = TipoPersona.Juridica,
                CondicionIva = CondicionIva.Exento
            },
            puntosVentaHabilitados: [1],
            puntosVentaDeshabilitados: [],
            _ => []))
        {
            cargados++;
        }

        return cargados;
    }

    // Factura A del responsable inscripto a la monotributista: productos con IVA 21%.
    private static Comprobante FacturaA(DateTime hoy) => new()
    {
        CUITEmisor = CuitResponsableInscripto,
        PuntoVentaNumero = 1,
        Tipo = TipoComprobante.FacturaA,
        Numero = 1,
        Concepto = ConceptoComprobante.Productos,
        Fecha = hoy,
        TipoDocumentoReceptor = TipoDocumento.CUIT,
        NumeroDocumentoReceptor = CuitMonotributista,
        CondicionIvaReceptor = CondicionIva.Monotributista,
        ImporteNeto = 10000m,
        ImporteIVA = 2100m,
        ImporteTotal = 12100m,
        AlicuotasIva = [new() { Alicuota = AlicuotaIva.GeneralVeintiuno, BaseImponible = 10000m, ImporteIVA = 2100m }]
    };

    // Factura B a un consumidor final sin identificar: productos con IVA 10,5%.
    private static Comprobante FacturaBAConsumidorFinal(DateTime hoy) => new()
    {
        CUITEmisor = CuitResponsableInscripto,
        PuntoVentaNumero = 1,
        Tipo = TipoComprobante.FacturaB,
        Numero = 1,
        Concepto = ConceptoComprobante.Productos,
        Fecha = hoy,
        TipoDocumentoReceptor = TipoDocumento.SinIdentificar,
        NumeroDocumentoReceptor = "0",
        CondicionIvaReceptor = CondicionIva.ConsumidorFinal,
        ImporteNeto = 1000m,
        ImporteIVA = 105m,
        ImporteTotal = 1105m,
        AlicuotasIva = [new() { Alicuota = AlicuotaIva.ReducidaDiezConCinco, BaseImponible = 1000m, ImporteIVA = 105m }]
    };

    // Factura C de la monotributista por servicios del mes anterior.
    private static Comprobante FacturaCDeServicios(DateTime hoy)
    {
        var inicioMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);

        return new Comprobante
        {
            CUITEmisor = CuitMonotributista,
            PuntoVentaNumero = 1,
            Tipo = TipoComprobante.FacturaC,
            Numero = 1,
            Concepto = ConceptoComprobante.Servicios,
            Fecha = hoy,
            FechaServicioDesde = inicioMesAnterior,
            FechaServicioHasta = inicioMesAnterior.AddMonths(1).AddDays(-1),
            FechaVencimientoPago = hoy.AddDays(10),
            TipoDocumentoReceptor = TipoDocumento.SinIdentificar,
            NumeroDocumentoReceptor = "0",
            CondicionIvaReceptor = CondicionIva.ConsumidorFinal,
            ImporteNeto = 5000m,
            ImporteTotal = 5000m
        };
    }

    private sealed class Cargador(ContribuyenteServicio contribuyentes,
        IContribuyenteRepositorio repositorioContribuyentes, PuntoVentaServicio puntosVenta,
        ComprobanteServicio comprobantes, DateTime hoy)
    {
        public bool CargarSiNoExiste(Contribuyente contribuyente, int[] puntosVentaHabilitados,
            int[] puntosVentaDeshabilitados, Func<DateTime, Comprobante[]> crearComprobantes)
        {
            if (contribuyentes.ConsultarPorCUIT(contribuyente.CUIT) is not null)
                return false;

            // Las personas humanas de prueba no pasan por RENAPER: son ficticias y no deben gastar créditos.
            if (contribuyente.TipoPersona == TipoPersona.Humana)
                repositorioContribuyentes.Agregar(contribuyente);
            else
                contribuyentes.Registrar(contribuyente);

            foreach (int numero in puntosVentaHabilitados)
                puntosVenta.Registrar(new PuntoVenta { CUITContribuyente = contribuyente.CUIT, Numero = numero });

            foreach (int numero in puntosVentaDeshabilitados)
            {
                puntosVenta.Registrar(new PuntoVenta
                {
                    CUITContribuyente = contribuyente.CUIT,
                    Numero = numero,
                    Habilitado = false
                });
            }

            foreach (var comprobante in crearComprobantes(hoy))
            {
                var resultado = comprobantes.Registrar(comprobante, hoy);

                if (!resultado.EsValido)
                {
                    throw new InvalidOperationException("Un comprobante de prueba no pasó las validaciones: " +
                        string.Join(", ", resultado.Errores.Select(e => $"{e.codigo} {e.mensaje}")));
                }

                comprobantes.AutorizarSimuladamente(comprobante.CUITEmisor, comprobante.PuntoVentaNumero,
                    comprobante.Tipo, comprobante.Numero);
            }

            return true;
        }
    }
}
