using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntergalaxyTech.Infrastructure.Repositories;

public class PersonajeRepository : Repository<Personaje>, IPersonajeRepository
{
    private readonly AppDbContext _context;

    public PersonajeRepository(AppDbContext context) : base(context) 
    { 
        _context = context;
    }

    public async Task<(IEnumerable<Personaje> Items, int TotalCount)> GetPagedAsync(string? nombre, string? estado, int page, int pageSize)
    {
        var query = _context.Personajes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(p => EF.Functions.Like(p.Nombre.ToLower(), $"%{nombre.ToLower()}%"));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(p => p.Estado.ToLower() == estado.ToLower());
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
