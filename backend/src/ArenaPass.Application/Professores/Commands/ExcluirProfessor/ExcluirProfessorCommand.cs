using MediatR;

namespace ArenaPass.Application.Professores.Commands.ExcluirProfessor;

public record ExcluirProfessorCommand(Guid ProfessorId) : IRequest;
