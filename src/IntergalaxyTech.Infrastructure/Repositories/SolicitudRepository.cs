using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Infrastructure.Data;

namespace IntergalaxyTech.Infrastructure.Repositories;

public class SolicitudRepository : Repository<Solicitud>
{
    public SolicitudRepository(AppDbContext context) : base(context) { }
}
