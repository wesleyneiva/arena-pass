using MediatR;

namespace ArenaPass.Application.Faturamento.Commands.MarcarFaturaPaga;

public record MarcarFaturaPagaCommand(Guid FaturaId, DateOnly? DataPagamento) : IRequest;
