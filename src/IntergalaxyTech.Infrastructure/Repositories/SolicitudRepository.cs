using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntergalaxyTech.Infrastructure.Repositories;

public class SolicitudRepository : Repository<Solicitud>, ISolicitudRepository
{
    private readonly AppDbContext _context;

    public SolicitudRepository(AppDbContext context) : base(context) 
    { 
        _context = context;
    }

    public async Task<(IEnumerable<Solicitud> Items, int TotalCount)> GetPagedAsync(string? estado, string? solicitante, int page, int pageSize)
    {
        var query = _context.Solicitudes.Include(s => s.PersonajeAsignado).AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (Enum.TryParse<IntergalaxyTech.Domain.Enums.EstadoSolicitud>(estado, true, out var estadoEnum))
            {
                query = query.Where(s => s.Estado == estadoEnum);
            }
        }

        if (!string.IsNullOrWhiteSpace(solicitante))
        {
            query = query.Where(s => EF.Functions.Like(s.Solicitante.ToLower(), $"%{solicitante.ToLower()}%"));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.FechaCreacion).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<ReporteSolicitudesDto> GetResumenAsync()
    {
        var solicitudes = await _context.Solicitudes.Include(s => s.PersonajeAsignado).ToListAsync();

        var totales = solicitudes.GroupBy(s => s.Estado.ToString())
                                 .ToDictionary(g => g.Key, g => g.Count());

        var personajeMasSolicitado = solicitudes.Where(s => s.PersonajeAsignado != null)
                                                .GroupBy(s => s.PersonajeAsignado!.Nombre)
                                                .OrderByDescending(g => g.Count())
                                                .Select(g => g.Key)
                                                .FirstOrDefault();

        return new ReporteSolicitudesDto
        {
            TotalesPorEstado = totales,
            PersonajeMasSolicitado = personajeMasSolicitado
        };
    }
}
