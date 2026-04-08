using IntergalaxyTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntergalaxyTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;

    public ReportesController(ISolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpGet("estado")]
    public async Task<ActionResult<Dictionary<string, int>>> Get()
    {
        var reporte = await _solicitudService.ObtenerReporteEstadosAsync();
        return Ok(reporte);
    }
}
