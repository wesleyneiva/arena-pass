using ArenaPass.Application.Convites.Dtos;
using MediatR;

namespace ArenaPass.Application.Convites.Queries.ListarConvitesDoAgendamento;

public record ListarConvitesDoAgendamentoQuery(Guid AgendamentoId, Guid? ProfessorId)
    : IRequest<List<ConviteResumoDto>>;
