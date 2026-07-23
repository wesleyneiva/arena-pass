using FluentValidation;

namespace ArenaPass.Application.Professores.Commands.AtualizarProfessor;

public class AtualizarProfessorCommandValidator : AbstractValidator<AtualizarProfessorCommand>
{
    public AtualizarProfessorCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Cpf).NotEmpty().Length(11).Matches("^[0-9]{11}$")
            .WithMessage("CPF deve conter 11 dígitos numéricos.");
    }
}
