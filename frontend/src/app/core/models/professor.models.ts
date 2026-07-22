export interface Professor {
  id: string;
  nome: string;
  email: string;
  cpf: string;
  statusAprovacao: 'Pendente' | 'Aprovado' | 'Suspenso';
}
