using FluentValidation;

namespace ArenaPass.Application.Auth.Commands.AtualizarPerfil;

public class AtualizarPerfilCommandValidator : AbstractValidator<AtualizarPerfilCommand>
{
    public AtualizarPerfilCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.SenhaAtual).NotEmpty();
        RuleFor(x => x.NovaSenha).MinimumLength(6).When(x => !string.IsNullOrEmpty(x.NovaSenha));
    }
}
