namespace ArenaPass.Application.Faturamento.Common;

// Status da fatura nunca é gravado — é sempre calculado a partir de
// DataPagamento/DataVencimento, então não existe transição de status que dependa de
// alguém rodar um job em segundo plano.
public static class FaturaStatusHelper
{
    public static string Calcular(DateOnly dataVencimento, DateOnly? dataPagamento, DateOnly hoje)
    {
        if (dataPagamento.HasValue)
        {
            return "Pago";
        }

        return dataVencimento < hoje ? "Atrasado" : "Pendente";
    }
}
