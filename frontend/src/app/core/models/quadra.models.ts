export interface Quadra {
  id: string;
  nome: string;
  modalidadeId: string;
  modalidadeNome: string;
  horaAbertura: string;
  horaFechamento: string;
  duracaoSlotMinutos: number;
  taxaPorHora: number;
  ativa: boolean;
}

export interface CriarQuadraRequest {
  nome: string;
  modalidadeNome: string;
  horaAbertura: string;
  horaFechamento: string;
  duracaoSlotMinutos: number;
  taxaPorHora: number;
}

export interface AtualizarQuadraRequest extends CriarQuadraRequest {
  ativa: boolean;
}

export interface HorarioSlot {
  horaInicio: string;
  horaFim: string;
  livre: boolean;
  agendamentoId: string | null;
}
