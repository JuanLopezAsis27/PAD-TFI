using Microsoft.AspNetCore.Mvc;

namespace ArcaSim.Api.Controllers;

/// <summary>
/// Endpoint de prueba para verificar que la API está levantada
/// (equivalente al método FEDummy de ARCA).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EstadoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { estado = "OK", fecha = DateTimeOffset.Now });
}
