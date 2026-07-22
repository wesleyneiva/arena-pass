import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Icon } from '../../../shared/icon/icon';

interface ItemMenu {
  rota: string;
  rotulo: string;
  icone: 'grid' | 'users' | 'calendar' | 'chart';
}

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html'
})
export class AdminLayout {
  readonly menuAberto = signal(false);

  readonly itens: ItemMenu[] = [
    { rota: '/admin/quadras', rotulo: 'Quadras', icone: 'grid' },
    { rota: '/admin/professores', rotulo: 'Professores', icone: 'users' },
    { rota: '/admin/agendamentos', rotulo: 'Agendamentos', icone: 'calendar' },
    { rota: '/admin/financeiro', rotulo: 'Financeiro', icone: 'chart' }
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
