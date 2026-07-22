using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Commands.MarcarRealizado;

public class MarcarAgendamentoRealizadoCommandHandler : IRequestHandler<MarcarAgendamentoRealizadoCommand>
{
    private readonly IApplicationDbContext _context;

    public MarcarAgendamentoRealizadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarcarAgendamentoRealizadoCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (agendamento.Status != StatusAgendamento.Confirmado)
        {
            throw new DomainException(
                $"Só é possível marcar como realizado um agendamento com status '{StatusAgendamento.Confirmado}'.");
        }

        agendamento.Status = StatusAgendamento.Realizado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
