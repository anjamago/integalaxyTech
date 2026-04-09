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
    public async Task<ActionResult<ApiResponse<IEnumerable<SolicitudDto>>>> Get()
    {
        var solicitudes = await _solicitudService.ObtenerTodasAsync();
        return Ok(ApiResponse<IEnumerable<SolicitudDto>>.Ok(solicitudes, "Solicitudes obtenidas correctamente."));
    }
}
