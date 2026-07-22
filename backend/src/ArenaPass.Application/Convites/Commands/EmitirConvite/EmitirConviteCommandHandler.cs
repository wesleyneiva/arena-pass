using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Convites.Commands.EmitirConvite;

public class EmitirConviteCommandHandler : IRequestHandler<EmitirConviteCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public EmitirConviteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(EmitirConviteCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (agendamento.ProfessorId != request.ProfessorId)
        {
            throw new UnauthorizedAccessException("Esse agendamento não pertence a você.");
        }

        if (agendamento.Status == StatusAgendamento.Cancelado)
        {
            throw new DomainException("Não é possível emitir convite para um agendamento cancelado.");
        }

        var convite = new Convite
        {
            AgendamentoId = agendamento.Id,
            AlunoNome = request.AlunoNome,
            AlunoCpf = request.AlunoCpf
        };

        _context.Convites.Add(convite);
        await _context.SaveChangesAsync(cancellationToken);

        return convite.Id;
    }
}
