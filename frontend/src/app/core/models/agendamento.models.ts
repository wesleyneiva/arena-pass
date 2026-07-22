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
}

export interface CriarAgendamentoRequest {
  quadraId: string;
  data: string;
  horaInicio: string;
  taxaValor: number;
}
