export type FormaPagamento = 'Pix' | 'Dinheiro' | 'Cartao';

export interface Agendamento {
  id: string;
  quadraId: string;
  quadraNome: string;
  professorId: string;
  professorNome: string;
  data: string;
  horaInicio: string;
  horaFim: string;
  status: 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';
  taxaValor: number;
  formaPagamento: FormaPagamento | null;
  encerrado: boolean;
}

export interface CriarAgendamentoRequest {
  quadraId: string;
  data: string;
  horaInicio: string;
}

export interface PagamentoPix {
  pixCopiaECola: string;
  qrCodeBase64: string;
}
