export interface Professor {
  id: string;
  nome: string;
  email: string;
  cpf: string;
  statusAprovacao: 'Pendente' | 'Aprovado' | 'Suspenso';
}

export interface CriarProfessorRequest {
  nome: string;
  email: string;
  senha: string;
  cpf: string;
}

export interface AtualizarProfessorRequest {
  nome: string;
  email: string;
  cpf: string;
}

export interface VerificarEmailProfessorResult {
  existe: boolean;
  nome: string | null;
  jaVinculado: boolean;
}
