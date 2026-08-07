using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Faturamento.Common;
using ArenaPass.Application.Faturamento.Dtos;
using ArenaPass.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Queries.ObterPainelFaturamento;

public class ObterPainelFaturamentoQueryHandler : IRequestHandler<ObterPainelFaturamentoQuery, PainelFaturamentoDto>
{
    private readonly IApplicationDbContext _context;

    public ObterPainelFaturamentoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PainelFaturamentoDto> Handle(ObterPainelFaturamentoQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(BrasilClock.Agora);
        var competencia = new DateOnly(hoje.Year, hoje.Month, 1);

        var espacos = await _context.Espacos.ToListAsync(cancellationToken);

        var assinaturas = await _context.Assinaturas
            .Where(a => a.Ativa)
            .Include(a => a.Plano)
            .ToListAsync(cancellationToken);

        // Garante a fatura da competência atual pra cada assinatura ativa antes de ler
        // — é aqui que o "job" de virada de mês na verdade acontece, sob demanda.
        foreach (var assinatura in assinaturas)
        {
            await GarantirFaturaHelper.GarantirFaturaDoMesAsync(_context, assinatura, hoje, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var faturasDoMes = await _context.Faturas
            .Where(f => f.Competencia == competencia)
            .ToListAsync(cancellationToken);

        var clientes = new List<EspacoFaturamentoDto>();
        var quantidadeEmDia = 0;
        var quantidadeAtrasados = 0;
        var receitaDoMes = 0m;

        foreach (var espaco in espacos)
        {
            var assinatura = assinaturas.FirstOrDefault(a => a.EspacoId == espaco.Id);
            if (assinatura is null)
            {
                clientes.Add(new EspacoFaturamentoDto(
                    espaco.Id, espaco.Nome, espaco.Ativo, null, null, null, "SemAssinatura", null, null, null));
                continue;
            }

            var fatura = faturasDoMes.First(f => f.AssinaturaId == assinatura.Id);
            var status = FaturaStatusHelper.Calcular(fatura.DataVencimento, fatura.DataPagamento, hoje);

            if (status == "Pago")
            {
                quantidadeEmDia++;
                receitaDoMes += fatura.Valor;
            }
            else if (status == "Atrasado")
            {
                quantidadeAtrasados++;
            }

            clientes.Add(new EspacoFaturamentoDto(
                espaco.Id,
                espaco.Nome,
                espaco.Ativo,
                assinatura.Plano?.Nome,
                assinatura.ValorMensal,
                assinatura.DiaVencimento,
                status,
                fatura.Id,
                fatura.DataVencimento,
                fatura.DataPagamento));
        }

        return new PainelFaturamentoDto(
            TotalEspacos: espacos.Count,
            EspacosAtivos: espacos.Count(e => e.Ativo),
            MrrTotal: assinaturas.Sum(a => a.ValorMensal),
            ReceitaDoMes: receitaDoMes,
            QuantidadeEmDia: quantidadeEmDia,
            QuantidadeAtrasados: quantidadeAtrasados,
            Clientes: clientes.OrderBy(c => c.EspacoNome).ToList());
    }
}
