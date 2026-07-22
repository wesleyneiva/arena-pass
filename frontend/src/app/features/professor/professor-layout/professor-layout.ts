import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Icon } from '../../../shared/icon/icon';

interface ItemMenu {
  rota: string;
  rotulo: string;
  icone: 'calendar' | 'clipboard';
}

@Component({
  selector: 'app-professor-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './professor-layout.html'
})
export class ProfessorLayout {
  readonly menuAberto = signal(false);

  readonly itens: ItemMenu[] = [
    { rota: '/professor/agendar', rotulo: 'Agendar aula', icone: 'calendar' },
    { rota: '/professor/meus-agendamentos', rotulo: 'Meus agendamentos', icone: 'clipboard' }
  ];

  constructor(
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
