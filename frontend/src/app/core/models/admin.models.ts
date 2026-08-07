export interface Admin {
  id: string;
  nome: string;
  email: string;
  espacoId: string | null;
}

export interface CriarAdminRequest {
  nome: string;
  email: string;
  senha: string;
  espacoId: string;
}

export interface AtualizarAdminRequest {
  nome: string;
  email: string;
}
