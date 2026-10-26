using ArcaSim.Api.Soap.Wsfev1;
using ArcaSim.Infraestructura;
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

builder.Services.AddInfraestructura(
    builder.Configuration.GetConnectionString("ArcaSimDb")
    ?? throw new InvalidOperationException("Falta la connection string 'ArcaSimDb'."));

var app = builder.Build();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
