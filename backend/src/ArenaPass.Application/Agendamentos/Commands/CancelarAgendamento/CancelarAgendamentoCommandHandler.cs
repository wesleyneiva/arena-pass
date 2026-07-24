using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Commands.CancelarAgendamento;

public class CancelarAgendamentoCommandHandler : IRequestHandler<CancelarAgendamentoCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelarAgendamentoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelarAgendamentoCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (request.SolicitanteProfessorId.HasValue
            && agendamento.ProfessorId != request.SolicitanteProfessorId.Value)
        {
            throw new UnauthorizedAccessException("Esse agendamento não pertence a você.");
        }

        if (agendamento.Status == StatusAgendamento.Cancelado)
        {
            throw new DomainException("Esse agendamento já está cancelado.");
        }

        if (BrasilClock.Agora > agendamento.Data.ToDateTime(agendamento.HoraFim))
        {
            throw new DomainException("Não é possível cancelar — o horário dessa aula já passou.");
        }

        agendamento.Status = StatusAgendamento.Cancelado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
