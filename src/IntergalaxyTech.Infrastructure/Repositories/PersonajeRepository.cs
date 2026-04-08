using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Infrastructure.Data;

namespace IntergalaxyTech.Infrastructure.Repositories;

public class PersonajeRepository : Repository<Personaje>
{
    public PersonajeRepository(AppDbContext context) : base(context) { }
}
