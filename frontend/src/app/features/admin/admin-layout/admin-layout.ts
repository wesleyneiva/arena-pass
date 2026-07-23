import { Component, computed, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Icon } from '../../../shared/icon/icon';

interface ItemMenu {
  rota: string;
  rotulo: string;
  icone: 'grid' | 'users' | 'calendar' | 'chart' | 'shield' | 'pencil';
}

const ITENS_BASE: ItemMenu[] = [
  { rota: '/admin/quadras', rotulo: 'Quadras', icone: 'grid' },
  { rota: '/admin/professores', rotulo: 'Professores', icone: 'users' },
  { rota: '/admin/agendamentos', rotulo: 'Agendamentos', icone: 'calendar' },
  { rota: '/admin/financeiro', rotulo: 'Financeiro', icone: 'chart' }
];

const ITEM_ADMINISTRADORES: ItemMenu = { rota: '/admin/administradores', rotulo: 'Administradores', icone: 'shield' };
const ITEM_PERFIL: ItemMenu = { rota: '/admin/perfil', rotulo: 'Meu perfil', icone: 'pencil' };

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html'
})
export class AdminLayout {
  readonly menuAberto = signal(false);

  readonly itens = computed<ItemMenu[]>(() => {
    const itens = [...ITENS_BASE];
    if (this.auth.role() === 'Master') {
      itens.push(ITEM_ADMINISTRADORES);
    }
    itens.push(ITEM_PERFIL);
    return itens;
  });

  constructor(
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
