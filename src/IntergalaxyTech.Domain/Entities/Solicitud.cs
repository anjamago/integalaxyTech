using System;
using IntergalaxyTech.Domain.Enums;

namespace IntergalaxyTech.Domain.Entities;

public class Solicitud
{
    public Guid Id { get; set; }
    public int PersonajeId { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string AccionRequerida { get; set; } = string.Empty;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation property
    public Personaje? PersonajeAsignado { get; set; }
}
