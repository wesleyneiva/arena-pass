using ArenaPass.Application.Agendamentos.Dtos;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoMensal;

public class ObterFaturamentoMensalQueryHandler
    : IRequestHandler<ObterFaturamentoMensalQuery, FaturamentoMensalDto>
{
    private readonly IApplicationDbContext _context;

    public ObterFaturamentoMensalQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaturamentoMensalDto> Handle(
        ObterFaturamentoMensalQuery request,
        CancellationToken cancellationToken)
    {
        var inicio = new DateOnly(request.Ano, request.Mes, 1);
        var fim = inicio.AddMonths(1);

        var agendamentosPagos = await _context.Agendamentos
            .Include(a => a.Professor).ThenInclude(p => p!.Usuario)
            .Where(a => a.Data >= inicio
                        && a.Data < fim
                        && (a.Status == StatusAgendamento.Confirmado || a.Status == StatusAgendamento.Realizado))
            .ToListAsync(cancellationToken);

        var porProfessor = agendamentosPagos
            .GroupBy(a => new { a.ProfessorId, Nome = a.Professor!.Usuario!.Nome })
            .Select(g => new FaturamentoProfessorDto(g.Key.ProfessorId, g.Key.Nome, g.Count(), g.Sum(a => a.TaxaValor)))
            .OrderByDescending(f => f.ValorTotal)
            .ToList();

        return new FaturamentoMensalDto(
            request.Ano,
            request.Mes,
            porProfessor.Sum(p => p.ValorTotal),
            porProfessor);
    }
}
