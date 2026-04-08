using FluentValidation;
using IntergalaxyTech.Application.DTOs;

namespace IntergalaxyTech.Application.Validators;

public class CrearSolicitudValidator : AbstractValidator<CrearSolicitudDto>
{
    public CrearSolicitudValidator()
    {
        RuleFor(x => x.Solicitante).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccionRequerida).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PersonajeId).GreaterThan(0);
    }
}
