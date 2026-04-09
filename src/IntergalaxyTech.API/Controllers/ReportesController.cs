using IntergalaxyTech.Application.DTOs;
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

    [HttpGet("solicitudes-resumen")]
    public async Task<ActionResult<ApiResponse<ReporteSolicitudesDto>>> GetReporteEstados()
    {
        var reporte = await _solicitudService.ObtenerReporteEstadosAsync();
        return Ok(ApiResponse<ReporteSolicitudesDto>.Ok(reporte, "Reporte generado exitosamente."));
    }
}
