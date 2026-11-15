using ArcaSim.Web.Components;
using ArcaSim.Web.Servicios;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Sesión con Clave Fiscal: la API valida la clave y devuelve un JWT; el portal lo guarda en una cookie
// cifrada (junto con la CUIT y el nombre) y lo manda en cada llamada a la API. Ver docs/clave-fiscal.md.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "ArcaSim.Sesion";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.LoginPath = "/ingresar";
        o.AccessDeniedPath = "/ingresar";
        o.ReturnUrlParameter = "returnUrl";
        // La cookie vence junto con el JWT (se fija al ingresar): no se renueva sola.
        o.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var apiUrlBase = builder.Configuration["Api:UrlBase"] ?? "http://localhost:5030";
builder.Services.AddHttpClient<ArcaSimApiCliente>(client =>
{
    client.BaseAddress = new Uri(apiUrlBase.TrimEnd('/') + "/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// "Salir": el formulario del menú lo envía con su token antiforgery, que se valida acá (así otro sitio no
// puede cerrarle la sesión a nadie).
app.MapPost("/salir", async (HttpContext contexto, IAntiforgery antiforgery) =>
{
    if (!await antiforgery.IsRequestValidAsync(contexto))
        return Results.BadRequest();

    await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("~/ingresar?salio=true");
});

app.Run();
