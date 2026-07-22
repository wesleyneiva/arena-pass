using FluentValidation;

namespace ArenaPass.Application.Quadras.Commands.CriarQuadra;

public class CriarQuadraCommandValidator : AbstractValidator<CriarQuadraCommand>
{
    public CriarQuadraCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ModalidadeNome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DuracaoSlotMinutos).GreaterThan(0);
        RuleFor(x => x.TaxaPorHora).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HoraFechamento)
            .GreaterThan(x => x.HoraAbertura)
            .WithMessage("Hora de fechamento deve ser depois da hora de abertura.");
    }
}
