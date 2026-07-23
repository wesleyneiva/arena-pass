using FluentValidation;

namespace ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;

public class ConfirmarCodigoRegistroProfessorCommandValidator : AbstractValidator<ConfirmarCodigoRegistroProfessorCommand>
{
    public ConfirmarCodigoRegistroProfessorCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Codigo).NotEmpty().Length(6);
    }
}
