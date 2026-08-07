using FluentValidation;

namespace ArenaPass.Application.Planos.Commands.CriarPlano;

public class CriarPlanoCommandValidator : AbstractValidator<CriarPlanoCommand>
{
    public CriarPlanoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ValorMensal).GreaterThan(0);
    }
}
