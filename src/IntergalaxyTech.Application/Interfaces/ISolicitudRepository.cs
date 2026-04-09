using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Domain.Entities;

namespace IntergalaxyTech.Application.Interfaces;

public interface ISolicitudRepository : IRepository<Solicitud>
{
    Task<(IEnumerable<Solicitud> Items, int TotalCount)> GetPagedAsync(string? estado, string? solicitante, int page, int pageSize);
    Task<ReporteSolicitudesDto> GetResumenAsync();
}
