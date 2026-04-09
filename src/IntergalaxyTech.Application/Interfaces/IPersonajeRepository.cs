using IntergalaxyTech.Domain.Entities;

namespace IntergalaxyTech.Application.Interfaces;

public interface IPersonajeRepository : IRepository<Personaje>
{
    Task<(IEnumerable<Personaje> Items, int TotalCount)> GetPagedAsync(string? nombre, string? estado, int page, int pageSize);
}
