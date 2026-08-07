export type StatusFatura = 'SemAssinatura' | 'Pago' | 'Pendente' | 'Atrasado';

export interface EspacoFaturamento {
  espacoId: string;
  espacoNome: string;
  espacoAtivo: boolean;
  planoNome: string | null;
  valorMensal: number | null;
  diaVencimento: number | null;
  status: StatusFatura;
  faturaAtualId: string | null;
  dataVencimentoAtual: string | null;
  dataPagamentoAtual: string | null;
}

export interface PainelFaturamento {
  totalEspacos: number;
  espacosAtivos: number;
  mrrTotal: number;
  receitaDoMes: number;
  quantidadeEmDia: number;
  quantidadeAtrasados: number;
  clientes: EspacoFaturamento[];
}

export interface Fatura {
  id: string;
  competencia: string;
  valor: number;
  dataVencimento: string;
  dataPagamento: string | null;
  status: StatusFatura;
}

export interface AtribuirAssinaturaRequest {
  planoId: string;
  diaVencimento: number;
}

export interface EstatisticasAnuais {
  ano: number;
  faturamentoPorMes: number[];
  novosClientesPorMes: number[];
  volumeContratadoPorMes: number[];
}
