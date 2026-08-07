export interface LoginRequest {
  email: string;
  senha: string;
}

export interface SolicitarCodigoRegistroProfessorRequest {
  nome: string;
  email: string;
  senha: string;
  cpf: string;
}

export interface ConfirmarCodigoRegistroProfessorRequest {
  email: string;
  codigo: string;
}

export interface AuthResult {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  role: 'AdminClube' | 'Professor' | 'Master';
  professorId: string | null;
  professorAprovado: boolean | null;
  espacoNome: string | null;
}

export interface AtualizarPerfilRequest {
  email: string;
  senhaAtual: string;
  novaSenha?: string;
}
