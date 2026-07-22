using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Agendamentos.Dtos;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Queries.ObterPagamentoPix;

public class ObterPagamentoPixQueryHandler : IRequestHandler<ObterPagamentoPixQuery, PagamentoPixDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPixPayloadGenerator _pixPayloadGenerator;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public ObterPagamentoPixQueryHandler(
        IApplicationDbContext context,
        IPixPayloadGenerator pixPayloadGenerator,
        IQrCodeGenerator qrCodeGenerator)
    {
        _context = context;
        _pixPayloadGenerator = pixPayloadGenerator;
        _qrCodeGenerator = qrCodeGenerator;
    }

    public async Task<PagamentoPixDto> Handle(ObterPagamentoPixQuery request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (agendamento.ProfessorId != request.ProfessorId)
        {
            throw new UnauthorizedAccessException("Esse agendamento não pertence a você.");
        }

        if (agendamento.Status != StatusAgendamento.PendentePagamento)
        {
            throw new DomainException("Esse agendamento não está aguardando pagamento.");
        }

        var payload = _pixPayloadGenerator.GerarPayload(agendamento.TaxaValor, agendamento.Id.ToString());
        var qrCodeBase64 = _qrCodeGenerator.GerarPngBase64(payload);

        return new PagamentoPixDto(payload, qrCodeBase64);
    }
}
