using ArenaPass.Application.Agendamentos.Dtos;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Queries.ListarTodosAgendamentos;

public class ListarTodosAgendamentosQueryHandler
    : IRequestHandler<ListarTodosAgendamentosQuery, List<AgendamentoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarTodosAgendamentosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgendamentoDto>> Handle(
        ListarTodosAgendamentosQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Agendamentos
            .Include(a => a.Quadra)
            .Include(a => a.Professor).ThenInclude(p => p!.Usuario)
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
