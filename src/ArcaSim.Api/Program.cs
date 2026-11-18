using ArcaSim.Api.Autenticacion;
using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Aplicacion.Modelos;
using ArcaSim.Aplicacion.Servicios;
using ArcaSim.Infraestructura;
using ArcaSim.Infraestructura.Integraciones.Renaper;
using ArcaSim.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);

// REST (lo usa el portal Blazor)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// SOAP (lo usan los sistemas de otros grupos, igual que con ARCA)
builder.Services.AddSoapCore();
builder.Services.AddScoped<IServiceSoap, ServiceSoap>();
// A futuro (opcional): registrar el servicio WSAA (loginCms) para autenticar con certificados

var renaper = builder.Configuration.GetSection(RenaperOpciones.Seccion).Get<RenaperOpciones>() ?? new RenaperOpciones();

builder.Services.AddInfraestructura(
    builder.Configuration.GetConnectionString("ArcaSimDb")
    ?? throw new InvalidOperationException("Falta la connection string 'ArcaSimDb'."),
    renaper);

// Sesión del portal: al ingresar con Clave Fiscal la API entrega un JWT, que el portal manda en cada
// llamada REST. El SOAP no lo usa: sigue autenticando con la API key del objeto Auth.
var jwt = builder.Configuration.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>() ?? new JwtOpciones();
jwt.Validar();
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<GeneradorTokenJwt>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;   // "sub" queda como "sub" (la CUIT/CUIL)
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emisor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = GeneradorTokenJwt.CrearClaveDeFirma(jwt.Clave),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// El QR de los comprobantes lleva a la constatación del portal (/fe/qr/, la misma ruta que ARCA). Sin
// Portal:UrlPublica llevaría a ARCA real, que no conoce los comprobantes del simulador.
var urlPortal = builder.Configuration["Portal:UrlPublica"];
var opcionesQr = string.IsNullOrWhiteSpace(urlPortal) ? OpcionesQr.Arca : OpcionesQr.ParaPortal(urlPortal);
builder.Services.AddSingleton(opcionesQr);

// Servicios de aplicación (casos de uso compartidos por REST y SOAP)
builder.Services.AddScoped<ClaveFiscalServicio>();
builder.Services.AddScoped<ContribuyenteServicio>();
builder.Services.AddScoped<PuntoVentaServicio>();
builder.Services.AddScoped<ComprobanteServicio>();

var app = builder.Build();

// Qué RENAPER se usa, para no tener que adivinarlo (la API key nunca se muestra).
if (renaper.Modo == ModoRenaper.Stub)
{
    app.Logger.LogInformation("RENAPER: modo Stub (simulado, sin red ni créditos). Para usar el real: " +
        "Renaper:Modo = Http y Renaper:ApiKey (ver docs/renaper.md).");
}
else
{
    app.Logger.LogInformation("RENAPER: modo Http contra {UrlBase} (API key de {Largo} caracteres).",
        renaper.UrlBase, renaper.ApiKey.Length);
}

if (opcionesQr.ApuntaAArca)
{
    app.Logger.LogWarning("Falta Portal:UrlPublica: el QR de los comprobantes lleva a ARCA real ({UrlBase}), " +
        "que no los conoce. Configurarla con la dirección pública del portal (Portal__UrlPublica).",
        opcionesQr.UrlBase);
}
else
{
    app.Logger.LogInformation("QR de los comprobantes: {UrlBase}?p=...", opcionesQr.UrlBase);
}

// Migraciones y datos de prueba al iniciar, según el ambiente (sección BaseDeDatos de appsettings).
app.Services.InicializarBaseDeDatos(
    aplicarMigraciones: app.Configuration.GetValue<bool>("BaseDeDatos:AplicarMigraciones"),
    cargarDatosDePrueba: app.Configuration.GetValue<bool>("BaseDeDatos:CargarDatosDePrueba"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference();    // /scalar (documentación interactiva)
}

// WSFEv1 en la misma ruta que ARCA. El WSDL se obtiene con /wsfev1/service.asmx?wsdl
((IApplicationBuilder)app).UseSoapEndpoint<IServiceSoap>(o =>
{
    o.Path = "/wsfev1/service.asmx";
    o.SoapSerializer = SoapSerializer.XmlSerializer;
    o.CaseInsensitivePath = true;
});
// A futuro (opcional): WSAA en /ws/services/LoginCms

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
