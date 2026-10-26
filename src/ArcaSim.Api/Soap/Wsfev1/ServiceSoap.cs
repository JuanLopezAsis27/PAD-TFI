namespace ArcaSim.Api.Soap.Wsfev1;

/// <summary>
/// Implementación del endpoint SOAP WSFEv1. Debe ser un adaptador delgado: valida el objeto Auth,
/// traduce el mensaje SOAP y llama a los servicios de ArcaSim.Aplicacion (la misma lógica que usa
/// la API REST del portal).
/// </summary>
public class ServiceSoap : IServiceSoap
{
    // TODO: informar el estado real de la base de datos y de la autenticación
    public DummyResponse FEDummy() => new() { AppServer = "OK", DbServer = "OK", AuthServer = "OK" };
}
