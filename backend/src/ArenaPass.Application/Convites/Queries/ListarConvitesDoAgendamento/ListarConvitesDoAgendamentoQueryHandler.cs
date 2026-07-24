using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Convites.Dtos;
using ArenaPass.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Convites.Queries.ListarConvitesDoAgendamento;

public class ListarConvitesDoAgendamentoQueryHandler
    : IRequestHandler<ListarConvitesDoAgendamentoQuery, List<ConviteResumoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarConvitesDoAgendamentoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConviteResumoDto>> Handle(
        ListarConvitesDoAgendamentoQuery request,
        CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Agendamento), request.AgendamentoId);

        if (request.ProfessorId.HasValue && agendamento.ProfessorId != request.ProfessorId.Value)
        {
            throw new UnauthorizedAccessException("Esse agendamento não pertence a você.");
        }

        return await _context.Convites
            .Where(c => c.AgendamentoId == request.AgendamentoId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConviteResumoDto(c.Id, c.AlunoNome, c.AlunoCpf, c.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
