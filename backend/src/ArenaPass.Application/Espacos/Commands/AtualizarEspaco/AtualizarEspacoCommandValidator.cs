using FluentValidation;

namespace ArenaPass.Application.Espacos.Commands.AtualizarEspaco;

public class AtualizarEspacoCommandValidator : AbstractValidator<AtualizarEspacoCommand>
{
    public AtualizarEspacoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subdominio)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Subdomínio deve conter apenas letras minúsculas, números e hífen.");
    }
}
