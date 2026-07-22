using MediatR;

namespace ArenaPass.Application.Professores.Commands.AprovarProfessor;

public record AprovarProfessorCommand(Guid ProfessorId) : IRequest;
