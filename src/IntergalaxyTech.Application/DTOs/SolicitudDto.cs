using System;
using IntergalaxyTech.Domain.Enums;

namespace IntergalaxyTech.Application.DTOs;

public class SolicitudDto
{
    public Guid Id { get; set; }
    public int PersonajeId { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string AccionRequerida { get; set; } = string.Empty;
    public EstadoSolicitud Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? MotivoRechazo { get; set; }
}

public class CrearSolicitudDto
{
    public int PersonajeId { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string AccionRequerida { get; set; } = string.Empty;
}

public class ActualizarEstadoSolicitudDto
{
    public EstadoSolicitud Estado { get; set; }
    public string? MotivoRechazo { get; set; }
}
