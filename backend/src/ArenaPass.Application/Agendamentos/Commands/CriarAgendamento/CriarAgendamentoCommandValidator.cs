using ArenaPass.Domain.Common;
using FluentValidation;

namespace ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;

public class CriarAgendamentoCommandValidator : AbstractValidator<CriarAgendamentoCommand>
{
    public CriarAgendamentoCommandValidator()
    {
        RuleFor(x => x.ProfessorId).NotEmpty();
        RuleFor(x => x.QuadraId).NotEmpty();
        RuleFor(x => x.Data).NotEqual(default(DateOnly));
        RuleFor(x => x.QuantidadeHoras)
            .InclusiveBetween(RegrasAgendamento.QuantidadeMinimaHoras, RegrasAgendamento.QuantidadeMaximaHoras)
            .WithMessage(
                $"Só é possível reservar de {RegrasAgendamento.QuantidadeMinimaHoras} a " +
                $"{RegrasAgendamento.QuantidadeMaximaHoras} hora(s) por vez.");
    }
}
