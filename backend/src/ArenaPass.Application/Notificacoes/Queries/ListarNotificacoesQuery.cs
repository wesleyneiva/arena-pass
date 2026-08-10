using ArenaPass.Application.Notificacoes.Dtos;
using MediatR;

namespace ArenaPass.Application.Notificacoes.Queries;

public record ListarNotificacoesQuery(int Limite = 20) : IRequest<PainelNotificacoesDto>;
