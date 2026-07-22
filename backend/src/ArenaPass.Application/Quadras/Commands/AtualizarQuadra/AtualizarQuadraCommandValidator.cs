using FluentValidation;

namespace ArenaPass.Application.Quadras.Commands.AtualizarQuadra;

public class AtualizarQuadraCommandValidator : AbstractValidator<AtualizarQuadraCommand>
{
    public AtualizarQuadraCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ModalidadeId).NotEmpty();
        RuleFor(x => x.DuracaoSlotMinutos).GreaterThan(0);
        RuleFor(x => x.TaxaPorHora).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HoraFechamento)
            .GreaterThan(x => x.HoraAbertura)
            .WithMessage("Hora de fechamento deve ser depois da hora de abertura.");
    }
}
