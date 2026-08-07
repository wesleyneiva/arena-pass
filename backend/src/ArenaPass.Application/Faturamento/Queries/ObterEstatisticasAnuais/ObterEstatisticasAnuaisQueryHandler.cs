using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Faturamento.Dtos;
using ArenaPass.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Queries.ObterEstatisticasAnuais;

// Faturamento por mês reflete só os meses em que alguém abriu o painel (as faturas
// são geradas sob demanda, ver GarantirFaturaHelper) — meses "vazios" no passado
// distante não significam necessariamente zero de receita, só que ninguém olhou o
// painel naquele mês. A partir de agora, como o dashboard é aberto regularmente, os
// dados passam a ficar completos mês a mês.
public class ObterEstatisticasAnuaisQueryHandler : IRequestHandler<ObterEstatisticasAnuaisQuery, EstatisticasAnuaisDto>
{
    private readonly IApplicationDbContext _context;

    public ObterEstatisticasAnuaisQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EstatisticasAnuaisDto> Handle(ObterEstatisticasAnuaisQuery request, CancellationToken cancellationToken)
    {
        var ano = BrasilClock.Agora.Year;

        var faturamentoPorMes = new decimal[12];
        var faturasPagas = await _context.Faturas
            .Where(f => f.Competencia.Year == ano && f.DataPagamento != null)
            .Select(f => new { f.Competencia.Month, f.Valor })
            .ToListAsync(cancellationToken);
        foreach (var grupo in faturasPagas.GroupBy(f => f.Month))
        {
            faturamentoPorMes[grupo.Key - 1] = grupo.Sum(f => f.Valor);
        }

        var novosClientesPorMes = new int[12];
        var espacosCriados = await _context.Espacos
            .Where(e => e.CreatedAt.Year == ano)
            .Select(e => e.CreatedAt.Month)
            .ToListAsync(cancellationToken);
        foreach (var grupo in espacosCriados.GroupBy(mes => mes))
        {
            novosClientesPorMes[grupo.Key - 1] = grupo.Count();
        }

        var volumeContratadoPorMes = new decimal[12];
        var assinaturasIniciadas = await _context.Assinaturas
            .Where(a => a.DataInicio.Year == ano)
            .Select(a => new { a.DataInicio.Month, a.ValorMensal })
            .ToListAsync(cancellationToken);
        foreach (var grupo in assinaturasIniciadas.GroupBy(a => a.Month))
        {
            volumeContratadoPorMes[grupo.Key - 1] = grupo.Sum(a => a.ValorMensal);
        }

        return new EstatisticasAnuaisDto(
            ano,
            faturamentoPorMes.ToList(),
            novosClientesPorMes.ToList(),
            volumeContratadoPorMes.ToList());
    }
}
