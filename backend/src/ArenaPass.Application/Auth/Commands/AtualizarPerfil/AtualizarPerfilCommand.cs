using MediatR;

namespace ArenaPass.Application.Auth.Commands.AtualizarPerfil;

public record AtualizarPerfilCommand(
    Guid UsuarioId,
    string Email,
    string SenhaAtual,
    string? NovaSenha) : IRequest;
