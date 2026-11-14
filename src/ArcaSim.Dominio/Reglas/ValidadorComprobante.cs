using System;
using System.Collections.Generic;
using System.Text;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorComprobante
{
    private const decimal toleranciaAbsoluta = 0.01m;
    private const decimal toleranciaRelativa = 0.0001m;

    public static bool EsValido(Comprobante? comprobante, Contribuyente? contribuyenteEmisor,
        CondicionIva condicionEmisor, DateTime fechaGeneracion, long ultimoNumeroAutorizado,
        DateTime? fechaUltimoAutorizado = null)
    {
        return Validar(comprobante, contribuyenteEmisor, condicionEmisor, fechaGeneracion,
            ultimoNumeroAutorizado, fechaUltimoAutorizado).EsValido;
    }

    public static ResultadoValidacion Validar(Comprobante? comprobante, Contribuyente? contribuyenteEmisor,
        CondicionIva condicionEmisor, DateTime fechaGeneracion, long ultimoNumeroAutorizado,
        DateTime? fechaUltimoAutorizado = null)
    {
        var errores = new List<ErrorValidacion>();

        if (comprobante is null)
        {
            return ResultadoValidacion.ConErrores(new ErrorValidacion("DOM-001",
                "El comprobante es obligatorio."));
        }

        var alicuotas = comprobante.AlicuotasIva;

        if (!ValidadorDatosComprobante.EsValido(comprobante))
        {
            errores.Add(new ErrorValidacion("DOM-002",
                "Los datos generales del comprobante son inválidos."));
        }

        var errorCondicion = ValidadorCondicionIvaReceptor.Validar(comprobante.CondicionIvaReceptor,
            comprobante.Tipo);

        if (errorCondicion is not null)
        {
            errores.Add(errorCondicion);
        }
        else
        {
            var condicionReceptor = comprobante.CondicionIvaReceptor!.Value;

            if (!ValidadorCompatibilidadComprobante.EsCompatibleReceptor(comprobante.Tipo,
                condicionReceptor))
            {
                errores.Add(new ErrorValidacion("10243",
                    "La condición de IVA del receptor no es válida para la clase de comprobante."));
            }
        }

        if (!ValidadorCompatibilidadComprobante.EsCompatibleEmisor(comprobante.Tipo, condicionEmisor))
        {
            errores.Add(new ErrorValidacion("DOM-003",
                "La condición fiscal del emisor no es compatible con el tipo de comprobante."));
        }

        if (EsComprobanteTipoA(comprobante.Tipo) &&
            comprobante.TipoDocumentoReceptor != TipoDocumento.CUIT)
        {
            errores.Add(new ErrorValidacion("10013",
                "Los comprobantes clase A requieren CUIT para el receptor."));
        }

        if (!ValidadorFechaComprobante.EsValida(comprobante.Fecha, fechaGeneracion, comprobante.Concepto,
            comprobante.FechaServicioDesde, comprobante.FechaServicioHasta,
            comprobante.FechaVencimientoPago))
        {
            errores.Add(new ErrorValidacion("DOM-004",
                "La fecha del comprobante o las fechas del servicio son inválidas."));
        }

        if (!ValidadorCorrelatividad.EsSiguienteNumero(ultimoNumeroAutorizado, comprobante.Numero))
        {
            errores.Add(new ErrorValidacion("10016", "El número del comprobante no es correlativo."));
        }

        if (fechaUltimoAutorizado.HasValue && comprobante.Fecha.Date < fechaUltimoAutorizado.Value.Date)
        {
            errores.Add(new ErrorValidacion("10016",
                "La fecha del comprobante no puede ser anterior a la del último comprobante autorizado."));
        }

        if (EsComprobanteTipoC(comprobante.Tipo))
        {
            // Los importes que deben ser cero en tipo C (10043, 10044, 10047) se controlan en
            // ValidadorImportesComprobante.
            if (alicuotas is { Count: > 0 })
            {
                errores.Add(new ErrorValidacion("10071",
                    "Los comprobantes tipo C no deben informar el array de IVA."));
            }
        }
        else
        {
            if (alicuotas is null)
            {
                errores.Add(new ErrorValidacion("DOM-007",
                    "La colección de alícuotas de IVA es obligatoria."));
            }
            else
            {
                bool permiteBaseCero = PermiteBaseImponibleCero(comprobante.Tipo);

                foreach (var detalle in alicuotas)
                {
                    errores.AddRange(ValidadorAlicuotaIva.Validar(detalle, permiteBaseCero));
                }

                if (alicuotas.GroupBy(x => x?.Alicuota).Any(g => g.Count() > 1))
                {
                    errores.Add(new ErrorValidacion("10022",
                        "No se permite repetir una misma alícuota de IVA."));
                }

                if (comprobante.ImporteNeto > 0 && alicuotas.Count == 0)
                {
                    errores.Add(new ErrorValidacion("DOM-008",
                        "Debe informar al menos una alícuota de IVA cuando corresponda."));
                }

                decimal sumaIVA = alicuotas.Where(x => x is not null).Sum(x => x!.ImporteIVA);
                decimal diferenciaIVA = Math.Abs(sumaIVA - comprobante.ImporteIVA);

                if (!CumpleTolerancia(diferenciaIVA, comprobante.ImporteIVA,
                    toleranciaAbsoluta * alicuotas.Count))
                {
                    errores.Add(new ErrorValidacion("10023",
                        "El importe de IVA no coincide con la suma de sus alícuotas."));
                }

                if (!EsNotaConExcepcionBaseImponible(comprobante.Tipo))
                {
                    decimal sumaBases = alicuotas.Where(x => x is not null).Sum(x => x!.BaseImponible);
                    decimal diferenciaBases = Math.Abs(sumaBases - comprobante.ImporteNeto);

                    if (!CumpleTolerancia(diferenciaBases, comprobante.ImporteNeto,
                        toleranciaAbsoluta * alicuotas.Count))
                    {
                        errores.Add(new ErrorValidacion("1408",
                            "La suma de las bases imponibles no coincide con el importe neto."));
                    }
                }
            }
        }

        decimal totalCalculado = EsComprobanteTipoC(comprobante.Tipo)
            ? comprobante.ImporteNeto + comprobante.ImporteTributos
            : comprobante.ImporteNeto + comprobante.ImporteNoGravado
                + comprobante.ImporteExento + comprobante.ImporteIVA
                + comprobante.ImporteTributos;

        if (!CumpleTolerancia(Math.Abs(totalCalculado - comprobante.ImporteTotal),
            comprobante.ImporteTotal, toleranciaAbsoluta))
        {
            errores.Add(new ErrorValidacion("10048",
                "El importe total no coincide con la suma de los importes."));
        }

        errores.AddRange(ValidadorImportesComprobante.Validar(comprobante));

        if (!ValidadorPuntoVenta.EsValido(comprobante.PuntoVenta, comprobante))
        {
            errores.Add(new ErrorValidacion("DOM-010",
                "El punto de venta es inexistente, está deshabilitado o no corresponde al emisor."));
        }

        if (!ValidadorRelacionPuntoVenta.EsValida(contribuyenteEmisor, comprobante.PuntoVenta))
        {
            errores.Add(new ErrorValidacion("DOM-011",
                "El punto de venta no pertenece al contribuyente emisor."));
        }

        return errores.Count == 0 ? ResultadoValidacion.Exito()
            : ResultadoValidacion.ConErrores(errores.ToArray());
    }

    private static bool CumpleTolerancia(decimal diferencia, decimal valorReferencia,
        decimal toleranciaAbsoluta)
    {
        return diferencia <= toleranciaAbsoluta || (valorReferencia != 0
                && diferencia / Math.Abs(valorReferencia) <= toleranciaRelativa);
    }

    private static bool PermiteBaseImponibleCero(TipoComprobante tipo)
    {
        return tipo is TipoComprobante.NotaDebitoA
            or TipoComprobante.NotaCreditoA
            or TipoComprobante.NotaDebitoB
            or TipoComprobante.NotaCreditoB;
    }

    private static bool EsNotaConExcepcionBaseImponible(TipoComprobante tipo)
    {
        return tipo is TipoComprobante.NotaDebitoA
            or TipoComprobante.NotaCreditoA
            or TipoComprobante.NotaDebitoB
            or TipoComprobante.NotaCreditoB;
    }

    private static bool EsComprobanteTipoA(TipoComprobante tipo)
    {
        return tipo is
            TipoComprobante.FacturaA or
            TipoComprobante.NotaDebitoA or
            TipoComprobante.NotaCreditoA;
    }

    private static bool EsComprobanteTipoC(TipoComprobante tipo)
    {
        return tipo is
            TipoComprobante.FacturaC or
            TipoComprobante.NotaDebitoC or
            TipoComprobante.NotaCreditoC;
    }
}