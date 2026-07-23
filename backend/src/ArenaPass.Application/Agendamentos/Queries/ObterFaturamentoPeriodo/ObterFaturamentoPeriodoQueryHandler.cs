using ArenaPass.Application.Agendamentos.Dtos;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoPeriodo;

public class ObterFaturamentoPeriodoQueryHandler
    : IRequestHandler<ObterFaturamentoPeriodoQuery, FaturamentoPeriodoDto>
{
    private readonly IApplicationDbContext _context;

    public ObterFaturamentoPeriodoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaturamentoPeriodoDto> Handle(
        ObterFaturamentoPeriodoQuery request,
        CancellationToken cancellationToken)
    {
        var agendamentosPagos = await _context.Agendamentos
            .Include(a => a.Professor).ThenInclude(p => p!.Usuario)
            .Where(a => a.Data >= request.DataInicio
                        && a.Data <= request.DataFim
                        && (a.Status == StatusAgendamento.Confirmado || a.Status == StatusAgendamento.Realizado)
                        && (request.ProfessorId == null || a.ProfessorId == request.ProfessorId))
            .ToListAsync(cancellationToken);

        var porProfessor = agendamentosPagos
            .GroupBy(a => new { a.ProfessorId, Nome = a.Professor!.Usuario!.Nome })
            .Select(g => new FaturamentoProfessorDto(g.Key.ProfessorId, g.Key.Nome, g.Count(), g.Sum(a => a.TaxaValor)))
            .OrderByDescending(f => f.ValorTotal)
            .ToList();

        var totalPorMes = agendamentosPagos
            .GroupBy(a => (a.Data.Year, a.Data.Month))
            .ToDictionary(g => g.Key, g => g.Sum(a => a.TaxaValor));

        var porMes = new List<FaturamentoMesDto>();
        var mesAtual = new DateOnly(request.DataInicio.Year, request.DataInicio.Month, 1);
        var mesLimite = new DateOnly(request.DataFim.Year, request.DataFim.Month, 1);

        while (mesAtual <= mesLimite)
        {
            var chave = (mesAtual.Year, mesAtual.Month);
            porMes.Add(new FaturamentoMesDto(
                mesAtual.Year,
                mesAtual.Month,
                totalPorMes.TryGetValue(chave, out var total) ? total : 0m));

            mesAtual = mesAtual.AddMonths(1);
        }

        return new FaturamentoPeriodoDto(
            request.DataInicio,
            request.DataFim,
            porProfessor.Sum(p => p.ValorTotal),
            porProfessor,
            porMes);
    }
}
