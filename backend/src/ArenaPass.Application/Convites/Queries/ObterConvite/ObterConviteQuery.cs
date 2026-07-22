using ArenaPass.Application.Convites.Dtos;
using MediatR;

namespace ArenaPass.Application.Convites.Queries.ObterConvite;

public record ObterConviteQuery(Guid ConviteId, Guid ProfessorId) : IRequest<ConviteDetalhesDto>;
