using MediatR;

namespace ArenaPass.Application.Professores.Commands.CriarProfessor;

public record CriarProfessorCommand(
    string Nome,
    string Email,
    string Senha,
    string Cpf) : IRequest<Guid>;
