using FluentValidation;

namespace ArenaPass.Application.Planos.Commands.AtualizarPlano;

public class AtualizarPlanoCommandValidator : AbstractValidator<AtualizarPlanoCommand>
{
    public AtualizarPlanoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ValorMensal).GreaterThan(0);
    }
}
