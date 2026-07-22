using MediatR;

namespace ArenaPass.Application.Professores.Commands.SuspenderProfessor;

public record SuspenderProfessorCommand(Guid ProfessorId) : IRequest;
