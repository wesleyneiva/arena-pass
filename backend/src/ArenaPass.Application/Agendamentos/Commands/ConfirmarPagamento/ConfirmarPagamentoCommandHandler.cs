using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Commands.ConfirmarPagamento;

public class ConfirmarPagamentoCommandHandler : IRequestHandler<ConfirmarPagamentoCommand>
{
    private readonly IApplicationDbContext _context;

    public ConfirmarPagamentoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (agendamento.Status != StatusAgendamento.PendentePagamento)
        {
            throw new DomainException(
                $"Só é possível confirmar pagamento de agendamentos com status '{StatusAgendamento.PendentePagamento}'.");
        }

        agendamento.Status = StatusAgendamento.Confirmado;
        agendamento.FormaPagamento = request.FormaPagamento;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
