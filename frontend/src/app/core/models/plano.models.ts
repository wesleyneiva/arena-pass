export interface Plano {
  id: string;
  nome: string;
  valorMensal: number;
  ativo: boolean;
}

export interface CriarPlanoRequest {
  nome: string;
  valorMensal: number;
}

export interface AtualizarPlanoRequest {
  nome: string;
  valorMensal: number;
}
