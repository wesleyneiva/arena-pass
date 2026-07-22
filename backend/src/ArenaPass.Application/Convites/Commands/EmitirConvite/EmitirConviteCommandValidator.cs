using FluentValidation;

namespace ArenaPass.Application.Convites.Commands.EmitirConvite;

public class EmitirConviteCommandValidator : AbstractValidator<EmitirConviteCommand>
{
    public EmitirConviteCommandValidator()
    {
        RuleFor(x => x.AgendamentoId).NotEmpty();
        RuleFor(x => x.ProfessorId).NotEmpty();
        RuleFor(x => x.AlunoNome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AlunoCpf).NotEmpty().Length(11).Matches("^[0-9]{11}$")
            .WithMessage("CPF deve conter 11 dígitos numéricos.");
    }
}
