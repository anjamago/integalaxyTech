using IntergalaxyTech.Application.DTOs;

namespace IntergalaxyTech.Application.Interfaces;

public interface ISolicitudService
{
    Task<SolicitudDto> CrearSolicitudAsync(CrearSolicitudDto peticion);
    Task ActualizarEstadoAsync(Guid id, ActualizarEstadoSolicitudDto peticion);
    Task<IEnumerable<SolicitudDto>> ObtenerTodasAsync();
    Task<Dictionary<string, int>> ObtenerReporteEstadosAsync();
}
