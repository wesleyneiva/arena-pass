using ArenaPass.Application.Professores.Dtos;
using MediatR;

namespace ArenaPass.Application.Professores.Queries;

public record VerificarEmailProfessorQuery(string Email) : IRequest<VerificarEmailProfessorDto>;
