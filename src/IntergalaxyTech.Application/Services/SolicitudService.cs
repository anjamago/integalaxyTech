using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Domain.Entities;
using IntergalaxyTech.Domain.Enums;

namespace IntergalaxyTech.Application.Services;

public class SolicitudService : ISolicitudService
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IRepository<Personaje> _personajeRepository;

    public SolicitudService(ISolicitudRepository solicitudRepository, IRepository<Personaje> personajeRepository)
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

        if (solicitud.Estado == EstadoSolicitud.Pendiente)
        {
            if (peticion.Estado != EstadoSolicitud.EnProceso && peticion.Estado != EstadoSolicitud.Rechazada)
                throw new InvalidOperationException("Solo se permite pasar de Pendiente a EnProceso o Rechazada.");
        }
        else if (solicitud.Estado == EstadoSolicitud.EnProceso)
        {
            if (peticion.Estado != EstadoSolicitud.Aprobada && peticion.Estado != EstadoSolicitud.Rechazada)
                throw new InvalidOperationException("Solo se permite pasar de EnProceso a Aprobada o Rechazada.");
        }
        else
        {
            throw new InvalidOperationException($"No se permite alterar una solicitud en estado {solicitud.Estado}.");
        }

        if (peticion.Estado == EstadoSolicitud.Rechazada && string.IsNullOrWhiteSpace(peticion.MotivoRechazo))
        {
            throw new InvalidOperationException("Se requiere proveer un motivo (MotivoRechazo) para Rechazarlas.");
        }

        solicitud.Estado = peticion.Estado;
        solicitud.MotivoRechazo = peticion.MotivoRechazo;
        solicitud.FechaActualizacion = DateTime.UtcNow;

        await _solicitudRepository.UpdateAsync(solicitud);
    }

    public async Task<PagedResult<SolicitudDto>> ObtenerTodasAsync(string? estado, string? solicitante, int page, int pageSize)
    {
        var result = await _solicitudRepository.GetPagedAsync(estado, solicitante, page, pageSize);
        return new PagedResult<SolicitudDto>
        {
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = pageSize,
            Items = result.Items.Select(ToDto)
        };
    }

    public async Task<ReporteSolicitudesDto> ObtenerReporteEstadosAsync()
    {
        return await _solicitudRepository.GetResumenAsync();
    }

    private static SolicitudDto ToDto(Solicitud solicitud) => new SolicitudDto
    {
        Id = solicitud.Id,
        PersonajeId = solicitud.PersonajeId,
        Solicitante = solicitud.Solicitante,
        AccionRequerida = solicitud.AccionRequerida,
        Estado = solicitud.Estado,
        FechaCreacion = solicitud.FechaCreacion,
        FechaActualizacion = solicitud.FechaActualizacion,
        MotivoRechazo = solicitud.MotivoRechazo
    };
}
