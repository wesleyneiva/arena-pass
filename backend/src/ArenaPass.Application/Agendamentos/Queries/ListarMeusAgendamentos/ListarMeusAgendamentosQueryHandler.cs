using ArenaPass.Application.Agendamentos.Dtos;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Queries.ListarMeusAgendamentos;

public class ListarMeusAgendamentosQueryHandler
    : IRequestHandler<ListarMeusAgendamentosQuery, List<AgendamentoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarMeusAgendamentosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgendamentoDto>> Handle(
        ListarMeusAgendamentosQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Agendamentos
            .Include(a => a.Quadra)
            .Include(a => a.Professor).ThenInclude(p => p!.Usuario)
            .Where(a => a.ProfessorId == request.ProfessorId)
            .OrderByDescending(a => a.Data).ThenBy(a => a.HoraInicio)
            .Select(a => new AgendamentoDto(
                a.Id,
                a.QuadraId,
                a.Quadra!.Nome,
                a.ProfessorId,
                a.Professor!.Usuario!.Nome,
                a.Data,
                a.HoraInicio,
                a.HoraFim,
                a.Status.ToString(),
                a.TaxaValor,
                a.FormaPagamento == null ? null : a.FormaPagamento.ToString()))
            .ToListAsync(cancellationToken);
    }
}
