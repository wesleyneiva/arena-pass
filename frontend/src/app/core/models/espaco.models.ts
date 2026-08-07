export interface ResolverEspacoResult {
  encontrado: boolean;
  nome: string | null;
}

export interface Espaco {
  id: string;
  nome: string;
  subdominio: string;
  ativo: boolean;
}

export interface CriarEspacoRequest {
  nome: string;
  subdominio: string;
}
