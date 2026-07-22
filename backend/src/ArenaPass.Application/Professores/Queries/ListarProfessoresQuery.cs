using ArenaPass.Application.Professores.Dtos;
using MediatR;

namespace ArenaPass.Application.Professores.Queries;

public record ListarProfessoresQuery : IRequest<List<ProfessorDto>>;
