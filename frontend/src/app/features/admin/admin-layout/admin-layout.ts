import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificacaoService } from '../../../core/services/notificacao.service';
import { Icon } from '../../../shared/icon/icon';

interface ItemMenu {
  rota: string;
  rotulo: string;
  icone: 'grid' | 'users' | 'calendar' | 'chart' | 'shield' | 'pencil' | 'clipboard';
}

const ITENS_BASE: ItemMenu[] = [
  { rota: '/admin/quadras', rotulo: 'Quadras', icone: 'grid' },
  { rota: '/admin/professores', rotulo: 'Professores', icone: 'users' },
  { rota: '/admin/agendamentos', rotulo: 'Agendamentos', icone: 'calendar' },
  { rota: '/admin/financeiro', rotulo: 'Financeiro', icone: 'chart' }
];

const ITEM_DASHBOARD: ItemMenu = { rota: '/admin/dashboard', rotulo: 'Dashboard', icone: 'chart' };
const ITEM_ESPACOS: ItemMenu = { rota: '/admin/espacos', rotulo: 'Espaços', icone: 'clipboard' };
const ITEM_PLANOS: ItemMenu = { rota: '/admin/planos', rotulo: 'Planos', icone: 'grid' };
const ITEM_ADMINISTRADORES: ItemMenu = { rota: '/admin/administradores', rotulo: 'Administradores', icone: 'shield' };
const ITEM_PERFIL: ItemMenu = { rota: '/admin/perfil', rotulo: 'Meu perfil', icone: 'pencil' };

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html'
})
export class AdminLayout implements OnInit, OnDestroy {
  readonly menuAberto = signal(false);

  readonly itens = computed<ItemMenu[]>(() => {
    if (this.auth.role() === 'Master') {
      return [ITEM_DASHBOARD, ITEM_ESPACOS, ITEM_PLANOS, ITEM_ADMINISTRADORES, ITEM_PERFIL];
    }
    return [...ITENS_BASE, ITEM_PERFIL];
  });

  constructor(
    readonly auth: AuthService,
    readonly notificacoes: NotificacaoService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    // Badge de notificações só faz sentido para o admin do espaço (Master não tem tenant).
    if (this.auth.role() === 'AdminClube') {
      this.notificacoes.iniciarPolling();
    }
  }

  ngOnDestroy(): void {
    this.notificacoes.pararPolling();
  }

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
