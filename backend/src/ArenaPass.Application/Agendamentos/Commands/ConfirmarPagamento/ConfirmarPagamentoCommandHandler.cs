using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Common;
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

        if (request.SolicitanteProfessorId.HasValue
            && agendamento.ProfessorId != request.SolicitanteProfessorId.Value)
        {
            throw new UnauthorizedAccessException("Esse agendamento não pertence a você.");
        }

        if (agendamento.Status != StatusAgendamento.PendentePagamento)
        {
            throw new DomainException(
                $"Só é possível confirmar pagamento de agendamentos com status '{StatusAgendamento.PendentePagamento}'.");
        }

        if (BrasilClock.Agora > agendamento.Data.ToDateTime(agendamento.HoraFim))
        {
            throw new DomainException("Não é possível confirmar pagamento — o horário dessa aula já passou.");
        }

        agendamento.Status = StatusAgendamento.Confirmado;
        agendamento.FormaPagamento = request.FormaPagamento;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
