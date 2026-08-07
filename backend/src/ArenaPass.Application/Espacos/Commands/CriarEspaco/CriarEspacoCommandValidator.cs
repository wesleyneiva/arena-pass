using FluentValidation;

namespace ArenaPass.Application.Espacos.Commands.CriarEspaco;

public class CriarEspacoCommandValidator : AbstractValidator<CriarEspacoCommand>
{
    public CriarEspacoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subdominio)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Subdomínio deve conter apenas letras minúsculas, números e hífen.");
    }
}
