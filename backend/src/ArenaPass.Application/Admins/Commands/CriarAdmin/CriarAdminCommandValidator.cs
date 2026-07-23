using FluentValidation;

namespace ArenaPass.Application.Admins.Commands.CriarAdmin;

public class CriarAdminCommandValidator : AbstractValidator<CriarAdminCommand>
{
    public CriarAdminCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6);
    }
}
