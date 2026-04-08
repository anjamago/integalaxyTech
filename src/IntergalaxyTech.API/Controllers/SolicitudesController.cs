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
    public async Task<ActionResult<SolicitudDto>> Create(CrearSolicitudDto dto)
    {
        var solicitud = await _solicitudService.CrearSolicitudAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = solicitud.Id }, solicitud);
    }

    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> UpdateStatus(Guid id, ActualizarEstadoSolicitudDto dto)
    {
        await _solicitudService.ActualizarEstadoAsync(id, dto);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudDto>>> Get()
    {
        var solicitudes = await _solicitudService.ObtenerTodasAsync();
        return Ok(solicitudes);
    }
}
