export interface FaturamentoProfessor {
  professorId: string;
  professorNome: string;
  totalAulas: number;
  valorTotal: number;
}

export interface FaturamentoMensal {
  ano: number;
  mes: number;
  totalGeral: number;
  porProfessor: FaturamentoProfessor[];
}
