using FluentValidation;

namespace ArenaPass.Application.Faturamento.Commands.AtribuirAssinatura;

public class AtribuirAssinaturaCommandValidator : AbstractValidator<AtribuirAssinaturaCommand>
{
    public AtribuirAssinaturaCommandValidator()
    {
        RuleFor(x => x.DiaVencimento).InclusiveBetween(1, 28);
    }
}
