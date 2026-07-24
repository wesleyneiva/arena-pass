export interface ConviteResumo {
  id: string;
  alunoNome: string;
  alunoCpf: string;
  status: 'Emitido' | 'Utilizado' | 'Expirado';
}

export interface ConviteDetalhes {
  id: string;
  alunoNome: string;
  alunoCpf: string;
  status: 'Emitido' | 'Utilizado' | 'Expirado';
  quadraNome: string;
  data: string;
  horaInicio: string;
  horaFim: string;
  validoDesde: string;
  qrCodeBase64: string;
}

export interface EmitirConviteRequest {
  alunoNome: string;
  alunoCpf: string;
}
