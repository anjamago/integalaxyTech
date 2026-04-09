using IntergalaxyTech.Application.DTOs;

namespace IntergalaxyTech.Application.Interfaces;

public interface ISolicitudService
{
    Task<SolicitudDto> CrearSolicitudAsync(CrearSolicitudDto peticion);
    Task ActualizarEstadoAsync(Guid id, ActualizarEstadoSolicitudDto peticion);
    Task<PagedResult<SolicitudDto>> ObtenerTodasAsync(string? estado, string? solicitante, int page, int pageSize);
    Task<SolicitudDto> ObtenerPorIdAsync(Guid id);
    Task<ReporteSolicitudesDto> ObtenerReporteEstadosAsync();
}
