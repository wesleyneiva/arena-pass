using FluentValidation;

namespace ArenaPass.Application.Auth.Commands.RegistrarProfessor;

public class RegistrarProfessorCommandValidator : AbstractValidator<RegistrarProfessorCommand>
{
    public RegistrarProfessorCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Cpf).NotEmpty().Length(11).Matches("^[0-9]{11}$")
            .WithMessage("CPF deve conter 11 dígitos numéricos.");
    }
}
