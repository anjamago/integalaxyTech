using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntergalaxyTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;

    public SolicitudesController(ISolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SolicitudDto>>> Create(CrearSolicitudDto dto)
    {
        var solicitud = await _solicitudService.CrearSolicitudAsync(dto);
        var response = ApiResponse<SolicitudDto>.Ok(solicitud, "Solicitud creada correctamente.");
        return CreatedAtAction(nameof(Get), new { id = solicitud.Id }, response);
    }

    [HttpPut("{id:guid}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, ActualizarEstadoSolicitudDto dto)
    {
        await _solicitudService.ActualizarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(null, "Estado actualizado correctamente."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SolicitudDto>>>> Get(
        [FromQuery] string? estado,
        [FromQuery] string? solicitante,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var solicitudes = await _solicitudService.ObtenerTodasAsync(estado, solicitante, page, pageSize);
        return Ok(ApiResponse<PagedResult<SolicitudDto>>.Ok(solicitudes, "Solicitudes obtenidas de manera paginada."));
    }
}
