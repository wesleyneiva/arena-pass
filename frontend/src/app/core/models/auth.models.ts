export interface LoginRequest {
  email: string;
  senha: string;
}

export interface RegistrarProfessorRequest {
  nome: string;
  email: string;
  senha: string;
  cpf: string;
}

export interface AuthResult {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  role: 'AdminClube' | 'Professor';
  professorId: string | null;
  professorAprovado: boolean | null;
}
