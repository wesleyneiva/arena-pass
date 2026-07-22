using MediatR;

namespace ArenaPass.Application.Professores.Commands.ReativarProfessor;

public record ReativarProfessorCommand(Guid ProfessorId) : IRequest;
