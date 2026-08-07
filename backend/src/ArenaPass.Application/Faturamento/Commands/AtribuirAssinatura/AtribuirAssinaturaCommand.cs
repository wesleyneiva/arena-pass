using MediatR;

namespace ArenaPass.Application.Faturamento.Commands.AtribuirAssinatura;

public record AtribuirAssinaturaCommand(Guid EspacoId, Guid PlanoId, int DiaVencimento) : IRequest;
