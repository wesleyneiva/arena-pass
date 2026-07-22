using MediatR;

namespace ArenaPass.Application.Auth.Commands.RegistrarProfessor;

public record RegistrarProfessorCommand(
    string Nome,
    string Email,
    string Senha,
    string Cpf) : IRequest<Guid>;
