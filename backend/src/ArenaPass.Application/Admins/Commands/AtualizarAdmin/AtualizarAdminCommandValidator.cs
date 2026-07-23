using FluentValidation;

namespace ArenaPass.Application.Admins.Commands.AtualizarAdmin;

public class AtualizarAdminCommandValidator : AbstractValidator<AtualizarAdminCommand>
{
    public AtualizarAdminCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
