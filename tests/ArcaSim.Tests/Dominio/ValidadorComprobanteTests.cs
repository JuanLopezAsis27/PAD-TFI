using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using ArcaSim.Dominio.Reglas;

namespace ArcaSim.Tests.Dominio;

public class ValidadorComprobanteTests
{
    private static Comprobante CrearComprobanteValido()
    {
        var comprobante = new Comprobante
        {
            CUITEmisor = "20301234563",
            PuntoVentaNumero = 1,
            PuntoVenta = new PuntoVenta
            {
                CUITContribuyente = "20301234563",
                Numero = 1,
                Habilitado = true
            },
            Tipo = TipoComprobante.FacturaA,
            Numero = 1,
            Fecha = DateTime.Today,
            Concepto = ConceptoComprobante.Productos,
            TipoDocumentoReceptor = TipoDocumento.CUIT,
            NumeroDocumentoReceptor = "27301234568",
            CondicionIvaReceptor = CondicionIva.ResponsableInscripto,
            ImporteNeto = 100m,
            ImporteIVA = 21m,
            ImporteTotal = 121m,
            Moneda = "PES",
            Cotizacion = 1m,
            AlicuotasIva =
            {
                new ComprobanteIva
                {
                    Alicuota = AlicuotaIva.GeneralVeintiuno,
                    BaseImponible = 100m,
                    ImporteIVA = 21m
                }
            },
        };
        return comprobante;
    }

    private static Contribuyente CrearContribuyenteValido()
    {
        var contribuyente = new Contribuyente
        {
            CUIT = "20301234563",
            NombreORazonSocial = "Contribuyente de prueba",
            TipoPersona = TipoPersona.Juridica,
            CondicionIva = CondicionIva.ResponsableInscripto
        };
        var puntoVenta = new PuntoVenta
        {
            CUITContribuyente = contribuyente.CUIT,
            Numero = 1,
            Habilitado = true,
            Contribuyente = contribuyente
        };
        contribuyente.PuntosVenta.Add(puntoVenta);
        return contribuyente;
    }

    [Fact]
    public void EsValido_ComprobanteCorrecto_DevuelveTrue()
    {
        var comprobante = CrearComprobanteValido();
        var contribuyente = CrearContribuyenteValido();
        bool resultado = ValidadorComprobante.EsValido(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void EsValido_TotalIncorrecto_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.ImporteTotal = 120000m;
        var contribuyente = CrearContribuyenteValido();
        bool resultado = ValidadorComprobante.EsValido(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.False(resultado);
    }

    [Fact]
    public void EsValido_NumeroNoCorrelativo_DevuelveFalse()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Numero = 2;
        var contribuyente = CrearContribuyenteValido();
        bool resultado = ValidadorComprobante.EsValido(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.False(resultado);
    }

    [Fact]
    public void Validar_ComprobanteNulo_DevuelveFalse()
    {
        var resultado = ValidadorComprobante.Validar(null, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-001");
    }

    [Fact]
    public void Validar_TotalIncorrecto_DevuelveCodigo10048()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.ImporteTotal = 130m;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.Contains(resultado.Errores, e => e.codigo == "10048");
    }

    [Fact]
    public void Validar_IvaNoCoincide_DevuelveCodigo10023()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.ImporteIVA = 20m;
        comprobante.ImporteTotal = 120m;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.Contains(resultado.Errores, e => e.codigo == "10023");
    }

    [Fact]
    public void Validar_FacturaAConDni_DevuelveCodigo10013()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.TipoDocumentoReceptor = TipoDocumento.DNI;
        comprobante.NumeroDocumentoReceptor = "12345678";
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, DateTime.Today, 0);
        Assert.Contains(resultado.Errores, e => e.codigo == "10013");
    }

    [Fact]
    public void Validar_CondicionReceptorInvalida_Devuelve10242()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.CondicionIvaReceptor = (CondicionIva)999;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10242");
    }

    [Fact]
    public void Validar_FacturaAConConsumidorFinal_Devuelve10243()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.CondicionIvaReceptor = CondicionIva.ConsumidorFinal;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10243");
    }

    [Fact]
    public void Validar_CondicionReceptorAusente_Devuelve10246()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.CondicionIvaReceptor = null;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            condicionEmisor: CondicionIva.ResponsableInscripto, fechaGeneracion: comprobante.Fecha,
            ultimoNumeroAutorizado: comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, error => error.codigo == "10246");
    }

    [Fact]
    public void Validar_ImporteNetoNegativo_DevuelveCodigo10045()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.ImporteNeto = -100m;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10045");
    }

    [Fact]
    public void Validar_PuntoVentaNulo_DevuelveDOM010()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.PuntoVenta = null;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-010");
    }

    [Fact]
    public void Validar_PuntoVentaDeshabilitado_DevuelveDOM010()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.PuntoVenta!.Habilitado = false;
        var contribuyente = CrearContribuyenteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-010");
    }

    [Fact]
    public void Validar_PuntoVentaAsociadoConOtroCUIT_DevuelveDOM011()
    {
        var comprobante = CrearComprobanteValido();
        var contribuyente = CrearContribuyenteValido();
        comprobante.PuntoVenta!.Contribuyente = new Contribuyente
        {
            CUIT = "27301234568"
        };
        var resultado = ValidadorComprobante.Validar(comprobante, contribuyente,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-011");
    }

    [Fact]
    public void Validar_ContribuyenteEmisorNulo_DevuelveDOM011()
    {
        var comprobante = CrearComprobanteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, null,
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-011");
    }

    [Fact]
    public void Validar_AlicuotasDuplicadas_DevuelveCodigo10022()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva.Add(new ComprobanteIva
        {
            Alicuota = AlicuotaIva.GeneralVeintiuno,
            BaseImponible = 100m,
            ImporteIVA = 21m
        });
        comprobante.ImporteIVA = 42m;
        comprobante.ImporteTotal = 142m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10022");
    }

    [Fact]
    public void Validar_SumaBasesImponiblesIncorrecta_DevuelveCodigo1408()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva[0].BaseImponible = 90m;
        comprobante.AlicuotasIva[0].ImporteIVA = 18.90m;
        comprobante.ImporteIVA = 18.90m;
        comprobante.ImporteTotal = 118.90m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "1408");
    }

    [Fact]
    public void Validar_NotaCreditoA_BaseImponibleCero_NoDevuelve10020y1408()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.NotaCreditoA;
        comprobante.AlicuotasIva[0].BaseImponible = 0m;
        comprobante.AlicuotasIva[0].ImporteIVA = 0m;
        comprobante.ImporteNeto = 100m;
        comprobante.ImporteIVA = 0m;
        comprobante.ImporteTotal = 100m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "10020");
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "1408");
    }

    [Fact]
    public void Validar_FacturaA_BaseImponibleCero_Devuelve10020()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva[0].BaseImponible = 0m;
        comprobante.AlicuotasIva[0].ImporteIVA = 0m;
        comprobante.ImporteIVA = 0m;
        comprobante.ImporteTotal = 100m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10020");
    }

    [Fact]
    public void Validar_ComprobanteCConIva_DevuelveCodigo10047()
    {
        var comprobante = CrearComprobanteValido();

        comprobante.Tipo = TipoComprobante.FacturaC;
        comprobante.ImporteIVA = 1m;
        comprobante.ImporteTotal = 101m;
        comprobante.AlicuotasIva.Clear();
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10047");
    }

    [Fact]
    public void Validar_ComprobanteCConAlicuotas_DevuelveCodigo10071()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaC;
        comprobante.ImporteIVA = 0m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10071");
    }

    [Fact]
    public void Validar_ComprobanteC_NoIncluyeImportesNoGravadosNiExentosEnTotal()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaC;
        comprobante.ImporteIVA = 0m;
        comprobante.AlicuotasIva.Clear();
        comprobante.ImporteNoGravado = 10m;
        comprobante.ImporteExento = 20m;
        comprobante.ImporteTributos = 5m;
        comprobante.ImporteTotal = 105m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "10048");
    }

    [Fact]
    public void Validar_ComprobanteCConTotalCorrecto_NoDevuelve10048()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaC;
        comprobante.ImporteIVA = 0m;
        comprobante.AlicuotasIva.Clear();
        comprobante.ImporteTotal = comprobante.ImporteNeto;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "10048");
    }

    [Fact]
    public void Validar_IvaDentroDeTolerancia_NoDevuelve10023()
    {
        var comprobante = CrearComprobanteValido();

        comprobante.AlicuotasIva[0].ImporteIVA = 21.01m;
        comprobante.ImporteIVA = 21.01m;
        comprobante.ImporteTotal = 121.01m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "10023");
    }

    [Fact]
    public void Validar_TotalDentroDeTolerancia_NoDevuelve10048()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.ImporteTotal = 121.01m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "10048");
    }

    [Fact]
    public void Validar_AlicuotaInvalida_DevuelveCodigo10019()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva[0].Alicuota = (AlicuotaIva)999;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10019");
    }

    [Fact]
    public void Validar_ImporteIvaNegativoEnDetalle_DevuelveCodigo10021()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva[0].ImporteIVA = -21m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10021");
    }

    [Fact]
    public void Validar_ImporteIvaNoCorrespondeALicuota_DevuelveCodigo10051()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.AlicuotasIva[0].ImporteIVA = 18m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10051");
    }

    [Fact]
    public void Validar_ServicioConFechasValidas_NoDevuelveDOM004()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Concepto = ConceptoComprobante.Servicios;
        comprobante.FechaServicioDesde = comprobante.Fecha.AddDays(-2);
        comprobante.FechaServicioHasta = comprobante.Fecha.AddDays(-1);
        comprobante.FechaVencimientoPago = comprobante.Fecha;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.DoesNotContain(resultado.Errores, e => e.codigo == "DOM-004");
    }

    [Fact]
    public void Validar_ServicioSinFechas_DevuelveDOM004()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Concepto = ConceptoComprobante.Servicios;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-004");
    }

    [Fact]
    public void Validar_ProductosConFechasDeServicio_DevuelveDOM004()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.FechaServicioDesde = comprobante.Fecha.AddDays(-2);
        comprobante.FechaServicioHasta = comprobante.Fecha.AddDays(-1);
        comprobante.FechaVencimientoPago = comprobante.Fecha;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-004");
    }

    [Fact]
    public void Validar_FacturaCConExento_DevuelveCodigo10044()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaC;
        comprobante.CondicionIvaReceptor = CondicionIva.ConsumidorFinal;
        comprobante.AlicuotasIva.Clear();
        comprobante.ImporteIVA = 0m;
        comprobante.ImporteExento = 50m;
        comprobante.ImporteTotal = 100m;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.Monotributista, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10044");
    }

    [Fact]
    public void Validar_FacturaBAConsumidorFinalSinIdentificar_DevuelveValido()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Tipo = TipoComprobante.FacturaB;
        comprobante.TipoDocumentoReceptor = TipoDocumento.SinIdentificar;
        comprobante.NumeroDocumentoReceptor = "0";
        comprobante.CondicionIvaReceptor = CondicionIva.ConsumidorFinal;
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void Validar_FacturaASinIdentificarReceptor_DevuelveCodigo10013()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.TipoDocumentoReceptor = TipoDocumento.SinIdentificar;
        comprobante.NumeroDocumentoReceptor = "0";
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "10013");
    }

    [Fact]
    public void Validar_FechaAnteriorAlUltimoAutorizado_DevuelveCodigo10016()
    {
        var comprobante = CrearComprobanteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1,
            comprobante.Fecha.AddDays(1));
        Assert.Contains(resultado.Errores, e => e.codigo == "10016");
    }

    [Fact]
    public void Validar_FechaIgualAlUltimoAutorizado_DevuelveValido()
    {
        var comprobante = CrearComprobanteValido();
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1,
            comprobante.Fecha);
        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void Validar_FechaInicioPosteriorAFinServicio_DevuelveDOM004()
    {
        var comprobante = CrearComprobanteValido();
        comprobante.Concepto = ConceptoComprobante.Servicios;
        comprobante.FechaServicioDesde = comprobante.Fecha.AddDays(2);
        comprobante.FechaServicioHasta = comprobante.Fecha.AddDays(1);
        comprobante.FechaVencimientoPago = comprobante.Fecha.AddDays(3);
        var resultado = ValidadorComprobante.Validar(comprobante, CrearContribuyenteValido(),
            CondicionIva.ResponsableInscripto, comprobante.Fecha, comprobante.Numero - 1);
        Assert.Contains(resultado.Errores, e => e.codigo == "DOM-004");
    }
}