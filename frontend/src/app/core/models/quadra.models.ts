export interface Quadra {
  id: string;
  nome: string;
  modalidadeId: string;
  modalidadeNome: string;
  horaAbertura: string;
  horaFechamento: string;
  duracaoSlotMinutos: number;
  ativa: boolean;
}

export interface CriarQuadraRequest {
  nome: string;
  modalidadeId: string;
  horaAbertura: string;
  horaFechamento: string;
  duracaoSlotMinutos: number;
}

export interface HorarioSlot {
  horaInicio: string;
  horaFim: string;
  livre: boolean;
  agendamentoId: string | null;
}
