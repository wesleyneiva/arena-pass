using ArenaPass.Application.Auth.Dtos;
using MediatR;

namespace ArenaPass.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Senha) : IRequest<AuthResultDto>;
