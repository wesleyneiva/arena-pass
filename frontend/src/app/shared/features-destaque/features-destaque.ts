import { Component } from '@angular/core';

interface Feature {
  titulo: string;
  descricao: string;
}

const FEATURES: Feature[] = [
  { titulo: 'Agendamento sem conflito', descricao: 'Bloqueio automático de horário — nunca duas aulas na mesma quadra.' },
  { titulo: 'Convite digital com QR Code', descricao: 'Aluno não-sócio entra só com o link da sua aula, sem cadastro.' },
  { titulo: 'Financeiro por professor', descricao: 'Pagamento via Pix e faturamento organizado automaticamente.' },
  { titulo: 'Um login, vários espaços', descricao: 'O mesmo professor dá aula em clubes diferentes sem duplicar cadastro.' }
];

@Component({
  selector: 'app-features-destaque',
  imports: [],
  templateUrl: './features-destaque.html'
})
export class FeaturesDestaque {
  readonly features = FEATURES;
}
