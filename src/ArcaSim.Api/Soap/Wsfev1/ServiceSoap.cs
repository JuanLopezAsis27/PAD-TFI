
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Dominio.Entidades;
using ArcaSim.Dominio.Enums;
using System.Globalization;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap : IServiceSoap
{
    private readonly ComprobanteServicio _comprobanteServicio;
    private readonly IValidadorApiKey _validadorApiKey;

    public ServiceSoap(
        ComprobanteServicio comprobanteServicio,
        IValidadorApiKey validadorApiKey)
    {
        _comprobanteServicio = comprobanteServicio;
        _validadorApiKey = validadorApiKey;
    }

    // =====================================================
    // AUTENTICACIÓN
    // =====================================================

    private SoapError? ValidarAutenticacion(FEAuthRequest? auth)
    {
        if (auth is null)
        {
            return new SoapError
            {
                Code = 600,
                Msg = "Debe informar las credenciales."
            };
        }

        if (!string.IsNullOrWhiteSpace(auth.Sign))
        {
            return new SoapError
            {
                Code = 600,
                Msg = "La autenticación WSAA aún no está disponible."
            };
        }

        var resultado = _validadorApiKey.Validar(
            auth.Token,
            auth.Cuit.ToString(CultureInfo.InvariantCulture));

        if (!resultado.EsValida)
        {
            return new SoapError
            {
                Code = resultado.CodigoError ?? 600,
                Msg = resultado.Mensaje ?? "Error de autenticación."
            };
        }

        return null;
    }

    // =====================================================
    // FEDummy
    // =====================================================

    public DummyResponse FEDummy() => new()
    {
        AppServer = "OK",
        DbServer = "OK",
        AuthServer = "OK"
    };

    // =====================================================
    // FECompUltimoAutorizado
    // =====================================================

    public FERecuperaLastCbteResponse FECompUltimoAutorizado(
        FEAuthRequest Auth,
        int PtoVta,
        int CbteTipo)
    {
        var respuesta = new FERecuperaLastCbteResponse
        {
            PtoVta = PtoVta,
            CbteTipo = CbteTipo
        };

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            return AgregarError(
                respuesta, errorAuth.Code, errorAuth.Msg);
        }

        if (PtoVta <= 0)
        {
            return AgregarError(
                respuesta, 400,
                "El punto de venta debe ser mayor que cero.");
        }

        if (!Enum.IsDefined(typeof(TipoComprobante), CbteTipo))
        {
            return AgregarError(
                respuesta, 400,
                "El tipo de comprobante no es válido.");
        }

        long ultimoNumero =
            _comprobanteServicio.ConsultarUltimoNumeroAutorizado(
                Auth.Cuit.ToString(CultureInfo.InvariantCulture),
                PtoVta,
                (TipoComprobante)CbteTipo);

        if (ultimoNumero < 0 || ultimoNumero > int.MaxValue)
        {
            return AgregarError(
                respuesta, 400,
                "Número autorizado fuera del rango permitido.");
        }

        respuesta.CbteNro = (int)ultimoNumero;

        return respuesta;
    }

    // =====================================================
    // FECompConsultar
    // =====================================================

    public FECompConsultaResponse FECompConsultar(
        FEAuthRequest Auth,
        FECompConsultaReq FeCompConsReq)
    {
        var respuesta = new FECompConsultaResponse();

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            return ErrorConsulta(
                respuesta, errorAuth.Code, errorAuth.Msg);
        }

        if (FeCompConsReq is null)
        {
            return ErrorConsulta(
                respuesta, 400,
                "Debe informar los datos del comprobante.");
        }

        if (FeCompConsReq.PtoVta <= 0 ||
            FeCompConsReq.CbteNro <= 0)
        {
            return ErrorConsulta(
                respuesta, 400,
                "Punto de venta o número inválido.");
        }

        if (!Enum.IsDefined(
            typeof(TipoComprobante),
            FeCompConsReq.CbteTipo))
        {
            return ErrorConsulta(
                respuesta, 400,
                "El tipo de comprobante no es válido.");
        }

        var comprobante = _comprobanteServicio.Consultar(
            Auth.Cuit.ToString(CultureInfo.InvariantCulture),
            FeCompConsReq.PtoVta,
            (TipoComprobante)FeCompConsReq.CbteTipo,
            FeCompConsReq.CbteNro);

        if (comprobante is null)
        {
            return ErrorConsulta(
                respuesta, 401,
                "El comprobante no existe.");
        }

        bool autorizado =
            comprobante.EstadoAutorizacion is
                EstadoAutorizacionComprobante.Autorizado or
                EstadoAutorizacionComprobante.AutorizadoConObservaciones;

        if (!autorizado)
        {
            return ErrorConsulta(
                respuesta, 401,
                "El comprobante no se encuentra autorizado.");
        }

        if (string.IsNullOrWhiteSpace(comprobante.CAE) ||
            !comprobante.FechaVencimientoCAE.HasValue)
        {
            return ErrorConsulta(
                respuesta, 401,
                "Faltan datos de autorización.");
        }

        if (!long.TryParse(
            comprobante.NumeroDocumentoReceptor,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out long documentoReceptor))
        {
            return ErrorConsulta(
                respuesta, 401,
                "El documento del receptor almacenado no es válido.");
        }

        respuesta.ResultGet = new FECompConsultaResult
        {
            Concepto = (int)comprobante.Concepto,
            DocTipo = (int)comprobante.TipoDocumentoReceptor,
            DocNro = documentoReceptor,
            CbteDesde = comprobante.Numero,
            CbteHasta = comprobante.Numero,

            CbteFch = comprobante.Fecha.ToString(
                "yyyyMMdd", CultureInfo.InvariantCulture),

            ImpTotal = (double)comprobante.ImporteTotal,
            ImpTotConc = (double)comprobante.ImporteNoGravado,
            ImpNeto = (double)comprobante.ImporteNeto,
            ImpOpEx = (double)comprobante.ImporteExento,
            ImpTrib = (double)comprobante.ImporteTributos,
            ImpIVA = (double)comprobante.ImporteIVA,

            FchServDesde =
                comprobante.FechaServicioDesde?.ToString(
                    "yyyyMMdd", CultureInfo.InvariantCulture),

            FchServHasta =
                comprobante.FechaServicioHasta?.ToString(
                    "yyyyMMdd", CultureInfo.InvariantCulture),

            FchVtoPago =
                comprobante.FechaVencimientoPago?.ToString(
                    "yyyyMMdd", CultureInfo.InvariantCulture),

            MonId = comprobante.Moneda,
            MonCotiz = (double)comprobante.Cotizacion,

            Resultado = "A",
            CodAutorizacion = comprobante.CAE,
            EmisionTipo = "CAE",

            FchVto =
                comprobante.FechaVencimientoCAE.Value.ToString(
                    "yyyyMMdd", CultureInfo.InvariantCulture),

            // La entidad todavía no expone la fecha real
            // de procesamiento/autorización.
            // Se omite el elemento si el modelo SOAP tiene
            // ShouldSerializeFchProceso().
            FchProceso = null,

            PtoVta = comprobante.PuntoVentaNumero,
            CbteTipo = (int)comprobante.Tipo
        };

        return respuesta;
    }

    // =====================================================
    // FECAESolicitar
    // =====================================================

    public FECAEResponse FECAESolicitar(
        FEAuthRequest Auth,
        FECAERequest FeCAEReq)
    {
        var respuesta = new FECAEResponse();

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            return ErrorCAE(
                respuesta, errorAuth.Code, errorAuth.Msg);
        }

        if (FeCAEReq is null ||
            FeCAEReq.FeCabReq is null ||
            FeCAEReq.FeDetReq is null)
        {
            return ErrorCAE(
                respuesta, 400,
                "La solicitud de autorización está incompleta.");
        }

        var cabecera = FeCAEReq.FeCabReq;

        if (cabecera.PtoVta <= 0 ||
            !Enum.IsDefined(
                typeof(TipoComprobante), cabecera.CbteTipo))
        {
            return ErrorCAE(
                respuesta, 400,
                "Punto de venta o tipo de comprobante inválido.");
        }

        if (cabecera.CantReg != 1 ||
            FeCAEReq.FeDetReq.Count != 1)
        {
            return ErrorCAE(
                respuesta, 400,
                "Esta versión admite un comprobante por solicitud.");
        }

        var detalle = FeCAEReq.FeDetReq[0];

        if (detalle is null ||
            detalle.CbteDesde <= 0 ||
            detalle.CbteDesde != detalle.CbteHasta ||
            detalle.DocNro < 0)
        {
            return ErrorCAE(
                respuesta, 400,
                "Identificación del comprobante inválida.");
        }

        if (!Enum.IsDefined(
            typeof(ConceptoComprobante), detalle.Concepto))
        {
            return ErrorCAE(
                respuesta, 400,
                "Concepto del comprobante inválido.");
        }

        if (!Enum.IsDefined(
            typeof(TipoDocumento), detalle.DocTipo))
        {
            return ErrorCAE(
                respuesta, 400,
                "Tipo de documento inválido.");
        }

        if ((detalle.CbtesAsoc?.Count ?? 0) > 0 ||
            (detalle.Tributos?.Count ?? 0) > 0 ||
            (detalle.Opcionales?.Count ?? 0) > 0 ||
            detalle.ImpTrib != 0)
        {
            return ErrorCAE(
                respuesta, 400,
                "Comprobantes asociados, tributos y campos " +
                "opcionales todavía no están soportados.");
        }

        if (!string.IsNullOrWhiteSpace(detalle.CanMisMonExt) &&
            detalle.CanMisMonExt != "S" &&
            detalle.CanMisMonExt != "N")
        {
            return ErrorCAE(
                respuesta, 400,
                "CanMisMonExt debe ser S o N.");
        }

        if (!TryFecha(detalle.CbteFch, out var fecha))
        {
            return ErrorCAE(
                respuesta, 400,
                "CbteFch debe tener formato yyyyMMdd.");
        }

        if (!TryFechaOpcional(
                detalle.FchServDesde, out var servicioDesde) ||
            !TryFechaOpcional(
                detalle.FchServHasta, out var servicioHasta) ||
            !TryFechaOpcional(
                detalle.FchVtoPago, out var vencimientoPago))
        {
            return ErrorCAE(
                respuesta, 400,
                "Las fechas deben tener formato yyyyMMdd.");
        }

        if (!TryDecimal(detalle.ImpTotal, out var total) ||
            !TryDecimal(detalle.ImpTotConc, out var noGravado) ||
            !TryDecimal(detalle.ImpNeto, out var neto) ||
            !TryDecimal(detalle.ImpOpEx, out var exento) ||
            !TryDecimal(detalle.ImpIVA, out var iva) ||
            !TryDecimal(detalle.MonCotiz, out var cotizacion))
        {
            return ErrorCAE(
                respuesta, 400,
                "Uno o más importes no son válidos.");
        }

        // =================================================
        // MAPEO DE CÓDIGOS DE CONDICIÓN IVA
        // =================================================

        CondicionIva? condicionReceptor = null;

        if (detalle.CondicionIVAReceptorId.HasValue)
        {
            if (!CondicionIvaArcaMapper.TryObtenerCondicion(
                detalle.CondicionIVAReceptorId.Value,
                out var condicion))
            {
                return ErrorCAE(
                    respuesta, 400,
                    "La condición IVA del receptor no está " +
                    "soportada por este simulador.");
            }

            condicionReceptor = condicion;
        }

        // =================================================
        // MAPEO DE ALÍCUOTAS
        // =================================================

        var alicuotas = new List<ComprobanteIva>();

        foreach (var item in detalle.Iva ?? new List<FEAlicIva>())
        {
            if (item is null ||
                !TryDecimal(item.BaseImp, out var baseImponible) ||
                !TryDecimal(item.Importe, out var importeIva))
            {
                return ErrorCAE(
                    respuesta, 400,
                    "Los datos de IVA son inválidos.");
            }

            var alicuota = (AlicuotaIva)Enum.ToObject(
                typeof(AlicuotaIva), (int)item.Id);

            if (!Enum.IsDefined(typeof(AlicuotaIva), alicuota))
            {
                return ErrorCAE(
                    respuesta, 400,
                    "La alícuota de IVA no existe.");
            }

            alicuotas.Add(new ComprobanteIva
            {
                Alicuota = alicuota,
                BaseImponible = baseImponible,
                ImporteIVA = importeIva
            });
        }

        // =================================================
        // CONSTRUCCIÓN DEL COMPROBANTE
        // =================================================

        var comprobante = new Comprobante
        {
            CUITEmisor = Auth.Cuit.ToString(
                CultureInfo.InvariantCulture),

            Concepto = (ConceptoComprobante)detalle.Concepto,
            PuntoVentaNumero = cabecera.PtoVta,
            Tipo = (TipoComprobante)cabecera.CbteTipo,
            Numero = detalle.CbteDesde,
            Fecha = fecha,

            FechaServicioDesde = servicioDesde,
            FechaServicioHasta = servicioHasta,
            FechaVencimientoPago = vencimientoPago,

            TipoDocumentoReceptor =
                (TipoDocumento)detalle.DocTipo,

            NumeroDocumentoReceptor =
                detalle.DocNro.ToString(
                    CultureInfo.InvariantCulture),

            CondicionIvaReceptor = condicionReceptor,

            ImporteNeto = neto,
            ImporteNoGravado = noGravado,
            ImporteExento = exento,
            ImporteTributos = 0m,
            ImporteIVA = iva,
            ImporteTotal = total,

            Moneda = detalle.MonId,
            Cotizacion = cotizacion,

            CancelaEnMismaMonedaExtranjera =
                detalle.CanMisMonExt == "S",

            AlicuotasIva = alicuotas
        };

        respuesta.FeCabResp = new FECAECabResponse
        {
            Cuit = Auth.Cuit,
            PtoVta = cabecera.PtoVta,
            CbteTipo = cabecera.CbteTipo,
            CantReg = 1,

            FchProceso = DateTime.Now.ToString(
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture),

            Resultado = "R",
            Reproceso = "N"
        };

        // =================================================
        // REGISTRAR Y AUTORIZAR CON EL ROL 1
        // =================================================

        var validacion = _comprobanteServicio.Registrar(
            comprobante,
            DateTime.Now);

        if (!validacion.EsValido)
        {
            respuesta.FeDetResp = new List<FECAEDetResponse>
            {
                CrearDetalleRespuesta(detalle, "R")
            };

            respuesta.Errors = validacion.Errores
                .Select(error => new SoapError
                {
                    Code = 400,
                    Msg = $"{error.codigo}: {error.mensaje}"
                })
                .ToList();

            return respuesta;
        }

        _comprobanteServicio.AutorizarSimuladamente(
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero);

        var autorizado = _comprobanteServicio.Consultar(
            comprobante.CUITEmisor,
            comprobante.PuntoVentaNumero,
            comprobante.Tipo,
            comprobante.Numero);

        if (autorizado is null ||
            string.IsNullOrWhiteSpace(autorizado.CAE) ||
            !autorizado.FechaVencimientoCAE.HasValue)
        {
            return ErrorCAE(
                respuesta, 500,
                "No se pudo confirmar la autorización. " +
                "Consulte el comprobante antes de reintentar.");
        }

        respuesta.FeCabResp.Resultado = "A";

        var detalleRespuesta =
            CrearDetalleRespuesta(detalle, "A");

        detalleRespuesta.CAE = autorizado.CAE;

        detalleRespuesta.CAEFchVto =
            autorizado.FechaVencimientoCAE.Value.ToString(
                "yyyyMMdd",
                CultureInfo.InvariantCulture);

        respuesta.FeDetResp = new List<FECAEDetResponse>
        {
            detalleRespuesta
        };

        return respuesta;
    }

    // =====================================================
    // FEParamGetTiposCbte
    // =====================================================

    public FEParamGetTiposCbteResponse FEParamGetTiposCbte(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetTiposCbteResponse();

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        respuesta.ResultGet = Enum
            .GetValues<TipoComprobante>()
            .Select(tipo => new FECbteTipo
            {
                Id = (int)tipo,
                Desc = tipo.ToString()
            })
            .OrderBy(item => item.Id)
            .ToList();

        return respuesta;
    }

    // =====================================================
    // FEParamGetTiposIva
    // =====================================================

    public FEParamGetTiposIvaResponse FEParamGetTiposIva(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetTiposIvaResponse();

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        respuesta.ResultGet = Enum
            .GetValues<AlicuotaIva>()
            .Select(tipo => new FEIvaTipo
            {
                Id = ((int)tipo).ToString(
                    CultureInfo.InvariantCulture),

                Desc = ObtenerDescripcionIva(tipo)
            })
            .OrderBy(item =>
                int.Parse(
                    item.Id,
                    CultureInfo.InvariantCulture))
            .ToList();

        return respuesta;
    }

    // =====================================================
    // FEParamGetCondicionIvaReceptor
    // =====================================================

    public FEParamGetCondicionIvaReceptorResponse
        FEParamGetCondicionIvaReceptor(
            FEAuthRequest Auth,
            string ClaseCmp)
    {
        var respuesta =
            new FEParamGetCondicionIvaReceptorResponse();

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        string clase =
            ClaseCmp?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (clase is not ("" or "A" or "B" or "C" or "M"))
        {
            respuesta.Errors = new List<SoapError>
            {
                new SoapError
                {
                    Code = 10244,
                    Msg = "La clase de comprobante no es válida."
                }
            };

            return respuesta;
        }

        // Catálogo reducido a las condiciones que
        // actualmente interpreta el dominio.

        var catalogo = new List<FECondicionIvaReceptor>
        {
            new()
            {
                Id = 1,
                Desc = "IVA Responsable Inscripto",
                CmpClase = "A"
            },
            new()
            {
                Id = 6,
                Desc = "Responsable Monotributo",
                CmpClase = "A"
            },
            new()
            {
                Id = 4,
                Desc = "IVA Sujeto Exento",
                CmpClase = "B"
            },
            new()
            {
                Id = 5,
                Desc = "Consumidor Final",
                CmpClase = "B"
            },
            new()
            {
                Id = 7,
                Desc = "Sujeto No Categorizado",
                CmpClase = "B"
            },
            new()
            {
                Id = 1,
                Desc = "IVA Responsable Inscripto",
                CmpClase = "C"
            },
            new()
            {
                Id = 4,
                Desc = "IVA Sujeto Exento",
                CmpClase = "C"
            },
            new()
            {
                Id = 5,
                Desc = "Consumidor Final",
                CmpClase = "C"
            },
            new()
            {
                Id = 6,
                Desc = "Responsable Monotributo",
                CmpClase = "C"
            },
            new()
            {
                Id = 7,
                Desc = "Sujeto No Categorizado",
                CmpClase = "C"
            }
        };

        respuesta.ResultGet = catalogo
            .Where(item =>
                string.IsNullOrEmpty(clase) ||
                item.CmpClase == clase)
            .OrderBy(item => item.CmpClase)
            .ThenBy(item => item.Id)
            .ToList();

        return respuesta;
    }

    // =====================================================
    // MÉTODOS AUXILIARES
    // =====================================================

    private static string ObtenerDescripcionIva(
        AlicuotaIva tipo)
    {
        return tipo switch
        {
            AlicuotaIva.Cero => "0%",
            AlicuotaIva.ReducidaDiezConCinco => "10,5%",
            AlicuotaIva.GeneralVeintiuno => "21%",
            AlicuotaIva.IncrementadaVeintisiete => "27%",
            AlicuotaIva.ReducidaCinco => "5%",
            AlicuotaIva.ReducidaDosConCinco => "2,5%",
            _ => tipo.ToString()
        };
    }

    private static bool TryDecimal(
        double valor,
        out decimal resultado)
    {
        resultado = 0;

        if (!double.IsFinite(valor))
        {
            return false;
        }

        try
        {
            resultado = Convert.ToDecimal(valor);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static bool TryFecha(
        string? valor,
        out DateTime fecha)
    {
        return DateTime.TryParseExact(
            valor,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);
    }

    private static bool TryFechaOpcional(
        string? valor,
        out DateTime? fecha)
    {
        fecha = null;

        if (string.IsNullOrWhiteSpace(valor))
        {
            return true;
        }

        if (!TryFecha(valor, out var fechaConvertida))
        {
            return false;
        }

        fecha = fechaConvertida;
        return true;
    }

    private static FECAEDetResponse CrearDetalleRespuesta(
        FECAEDetRequest detalle,
        string resultado)
    {
        return new FECAEDetResponse
        {
            Concepto = detalle.Concepto,
            DocTipo = detalle.DocTipo,
            DocNro = detalle.DocNro,
            CbteDesde = detalle.CbteDesde,
            CbteHasta = detalle.CbteHasta,
            CbteFch = detalle.CbteFch,
            Resultado = resultado
        };
    }

    private static FECAEResponse ErrorCAE(
        FECAEResponse respuesta,
        int codigo,
        string mensaje)
    {
        respuesta.Errors = new List<SoapError>
        {
            new SoapError
            {
                Code = codigo,
                Msg = mensaje
            }
        };

        if (respuesta.FeCabResp is not null)
        {
            respuesta.FeCabResp.Resultado = "R";
        }

        return respuesta;
    }

    private static FERecuperaLastCbteResponse AgregarError(
        FERecuperaLastCbteResponse respuesta,
        int codigo,
        string mensaje)
    {
        respuesta.Errors = new List<SoapError>
        {
            new SoapError
            {
                Code = codigo,
                Msg = mensaje
            }
        };

        return respuesta;
    }

    private static FECompConsultaResponse ErrorConsulta(
        FECompConsultaResponse respuesta,
        int codigo,
        string mensaje)
    {
        respuesta.Errors = new List<SoapError>
        {
            new SoapError
            {
                Code = codigo,
                Msg = mensaje
            }
        };

        return respuesta;
    }
}
