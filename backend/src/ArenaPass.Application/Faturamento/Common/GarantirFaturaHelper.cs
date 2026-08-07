using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Common;

// Gera a fatura da competência (mês) atual sob demanda — sem job agendado, a fatura
// só passa a existir quando alguém (o Master, ao abrir o painel) efetivamente olha
// pra aquela assinatura. Não salva — quem chama decide quando dar SaveChangesAsync.
public static class GarantirFaturaHelper
{
    public static async Task<Fatura> GarantirFaturaDoMesAsync(
        IApplicationDbContext context,
        Assinatura assinatura,
        DateOnly hoje,
        CancellationToken cancellationToken)
    {
        var competencia = new DateOnly(hoje.Year, hoje.Month, 1);

        var fatura = await context.Faturas.FirstOrDefaultAsync(
            f => f.AssinaturaId == assinatura.Id && f.Competencia == competencia,
            cancellationToken);

        if (fatura is not null)
        {
            return fatura;
        }

        fatura = new Fatura
        {
            AssinaturaId = assinatura.Id,
            EspacoId = assinatura.EspacoId,
            Competencia = competencia,
            Valor = assinatura.ValorMensal,
            DataVencimento = new DateOnly(hoje.Year, hoje.Month, assinatura.DiaVencimento)
        };

        context.Faturas.Add(fatura);
        return fatura;
    }
}
