export interface FaturamentoProfessor {
  professorId: string;
  professorNome: string;
  totalAulas: number;
  valorTotal: number;
}

export interface FaturamentoMes {
  ano: number;
  mes: number;
  total: number;
}

export interface FaturamentoPeriodo {
  dataInicio: string;
  dataFim: string;
  totalGeral: number;
  porProfessor: FaturamentoProfessor[];
  porMes: FaturamentoMes[];
}
