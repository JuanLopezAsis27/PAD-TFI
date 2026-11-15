
using ArcaSim.Api.Soap.Wsfev1.Models;
using ArcaSim.Aplicacion.Interfaces;
using ArcaSim.Aplicacion.Servicios;

namespace ArcaSim.Api.Soap.Wsfev1;

public partial class ServiceSoap
{
    // =====================================================
    // REPOSITORIO DE PUNTOS DE VENTA
    // =====================================================

    private readonly IPuntoVentaRepositorio?
        _puntoVentaRepositorio;

    // =====================================================
    // CONSTRUCTOR CON REPOSITORIO
    // =====================================================

    public ServiceSoap(
        ComprobanteServicio comprobanteServicio,
        IValidadorApiKey validadorApiKey,
        IPuntoVentaRepositorio puntoVentaRepositorio)
        : this(comprobanteServicio, validadorApiKey)
    {
        _puntoVentaRepositorio = puntoVentaRepositorio;
    }

    // =====================================================
    // FEParamGetPtosVenta
    // =====================================================

    public FEParamGetPtosVentaResponse FEParamGetPtosVenta(
        FEAuthRequest Auth)
    {
        var respuesta = new FEParamGetPtosVentaResponse();

        // ================================================
        // 1. VALIDAR AUTENTICACIÓN
        // ================================================

        var errorAuth = ValidarAutenticacion(Auth);

        if (errorAuth is not null)
        {
            respuesta.Errors = new List<SoapError>
            {
                errorAuth
            };

            return respuesta;
        }

        // ================================================
        // 2. COMPROBAR REPOSITORIO
        // ================================================

        if (_puntoVentaRepositorio is null)
        {
            respuesta.Errors = new List<SoapError>
            {
                new SoapError
                {
                    Code = 500,
                    Msg = "El repositorio de puntos de " +
                          "venta no está configurado."
                }
            };

            return respuesta;
        }

        // ================================================
        // 3. OBTENER PUNTOS DE VENTA DE LA CUIT
        // ================================================

        string cuit = Auth.Cuit.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        var puntosVenta =
            _puntoVentaRepositorio.ObtenerPorCuit(cuit);

        // ================================================
        // 4. CONSTRUIR RESPUESTA SOAP
        // ================================================

        respuesta.ResultGet = puntosVenta
            .Select(puntoVenta => new FEPtoVenta
            {
                Nro = puntoVenta.Numero,

                // Nuestro simulador utiliza CAE.
                EmisionTipo = "CAE",

                // Traducir el estado interno.
                Bloqueado = puntoVenta.Habilitado
                    ? "N"
                    : "S",

                // El dominio no almacena fecha de baja.
                FchBaja = null
            })
            .ToList();

        return respuesta;
    }
}
