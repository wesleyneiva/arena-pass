using FluentValidation;

namespace ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;

public class CriarAgendamentoCommandValidator : AbstractValidator<CriarAgendamentoCommand>
{
    public CriarAgendamentoCommandValidator()
    {
        RuleFor(x => x.ProfessorId).NotEmpty();
        RuleFor(x => x.QuadraId).NotEmpty();
        RuleFor(x => x.Data).NotEqual(default(DateOnly));
        RuleFor(x => x.TaxaValor).GreaterThanOrEqualTo(0);
    }
}
