using FluentValidation;

namespace ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoPeriodo;

public class ObterFaturamentoPeriodoQueryValidator : AbstractValidator<ObterFaturamentoPeriodoQuery>
{
    public ObterFaturamentoPeriodoQueryValidator()
    {
        RuleFor(x => x.DataFim)
            .GreaterThanOrEqualTo(x => x.DataInicio)
            .WithMessage("A data final deve ser igual ou depois da data inicial.");
    }
}
