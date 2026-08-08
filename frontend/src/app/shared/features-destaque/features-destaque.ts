import { Component } from '@angular/core';
import { Icon, IconName } from '../icon/icon';

interface Feature {
  icone: IconName;
  titulo: string;
  descricao: string;
}

const FEATURES: Feature[] = [
  { icone: 'calendar', titulo: 'Agendamento sem conflito', descricao: 'Bloqueio automático de horário — nunca duas aulas na mesma quadra.' },
  { icone: 'qrcode', titulo: 'Convite digital com QR Code', descricao: 'Aluno não-sócio entra só com o link da sua aula, sem cadastro.' },
  { icone: 'chart', titulo: 'Financeiro por professor', descricao: 'Pagamento via Pix e faturamento organizado automaticamente.' },
  { icone: 'users', titulo: 'Um login, vários espaços', descricao: 'O mesmo professor dá aula em clubes diferentes sem duplicar cadastro.' }
];

@Component({
  selector: 'app-features-destaque',
  imports: [Icon],
  templateUrl: './features-destaque.html'
})
export class FeaturesDestaque {
  readonly features = FEATURES;
}
