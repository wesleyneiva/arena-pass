using MediatR;

namespace ArenaPass.Application.Professores.Commands.AtualizarProfessor;

public record AtualizarProfessorCommand(
    Guid ProfessorId,
    string Nome,
    string Email,
    string Cpf) : IRequest;
