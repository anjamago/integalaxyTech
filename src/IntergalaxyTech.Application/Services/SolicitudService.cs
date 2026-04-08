using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Domain.Enums;

namespace IntergalaxyTech.Application.Services;

public class SolicitudService : ISolicitudService
{
    private readonly IRepository<Solicitud> _solicitudRepository;
    private readonly IRepository<Personaje> _personajeRepository;

    public SolicitudService(IRepository<Solicitud> solicitudRepository, IRepository<Personaje> personajeRepository)
    {
        _solicitudRepository = solicitudRepository;
        _personajeRepository = personajeRepository;
    }

    public async Task<SolicitudDto> CrearSolicitudAsync(CrearSolicitudDto peticion)
    {
        var personaje = await _personajeRepository.GetByIdAsync(peticion.PersonajeId);
        if (personaje == null)
            throw new ArgumentException("Personaje no encontrado en la base de datos local.");

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            PersonajeId = peticion.PersonajeId,
            Solicitante = peticion.Solicitante,
            AccionRequerida = peticion.AccionRequerida,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        await _solicitudRepository.AddAsync(solicitud);

        return ToDto(solicitud);
    }

    public async Task ActualizarEstadoAsync(Guid id, ActualizarEstadoSolicitudDto peticion)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(id);
        if (solicitud == null)
            throw new KeyNotFoundException("Solicitud no encontrada.");

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
            throw new InvalidOperationException("Solo se pueden modificar solicitudes en estado Pendiente.");

        solicitud.Estado = peticion.Estado;
        solicitud.FechaActualizacion = DateTime.UtcNow;

        await _solicitudRepository.UpdateAsync(solicitud);
    }

    public async Task<IEnumerable<SolicitudDto>> ObtenerTodasAsync()
    {
        var solicitudes = await _solicitudRepository.GetAllAsync();
        return solicitudes.Select(ToDto);
    }

    public async Task<Dictionary<string, int>> ObtenerReporteEstadosAsync()
    {
        var solicitudes = await _solicitudRepository.GetAllAsync();
        return solicitudes
            .GroupBy(s => s.Estado.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static SolicitudDto ToDto(Solicitud solicitud) => new SolicitudDto
    {
        Id = solicitud.Id,
        PersonajeId = solicitud.PersonajeId,
        Solicitante = solicitud.Solicitante,
        AccionRequerida = solicitud.AccionRequerida,
        Estado = solicitud.Estado,
        FechaCreacion = solicitud.FechaCreacion,
        FechaActualizacion = solicitud.FechaActualizacion
    };
}
