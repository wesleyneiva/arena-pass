export interface Admin {
  id: string;
  nome: string;
  email: string;
}

export interface CriarAdminRequest {
  nome: string;
  email: string;
  senha: string;
}

export interface AtualizarAdminRequest {
  nome: string;
  email: string;
}
