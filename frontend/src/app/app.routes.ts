import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'registrar',
    loadComponent: () => import('./features/auth/registrar/registrar').then((m) => m.Registrar)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard(['AdminClube', 'Master'])],
    loadComponent: () =>
      import('./features/admin/admin-layout/admin-layout').then((m) => m.AdminLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'quadras' },
      {
        path: 'quadras',
        loadComponent: () =>
          import('./features/admin/quadras-admin/quadras-admin').then((m) => m.QuadrasAdmin)
      },
      {
        path: 'professores',
        loadComponent: () =>
          import('./features/admin/professores-admin/professores-admin').then(
            (m) => m.ProfessoresAdmin
          )
      },
      {
        path: 'agendamentos',
        loadComponent: () =>
          import('./features/admin/agendamentos-admin/agendamentos-admin').then(
            (m) => m.AgendamentosAdmin
          )
      },
      {
        path: 'financeiro',
        loadComponent: () =>
          import('./features/admin/financeiro-admin/financeiro-admin').then(
            (m) => m.FinanceiroAdmin
          )
      },
      {
        path: 'administradores',
        canActivate: [roleGuard('Master')],
        loadComponent: () =>
          import('./features/admin/admins-master/admins-master').then((m) => m.AdminsMaster)
      },
      {
        path: 'perfil',
        loadComponent: () =>
          import('./features/admin/perfil-admin/perfil-admin').then((m) => m.PerfilAdmin)
      }
    ]
  },
  {
    path: 'professor',
    canActivate: [authGuard, roleGuard('Professor')],
    loadComponent: () =>
      import('./features/professor/professor-layout/professor-layout').then(
        (m) => m.ProfessorLayout
      ),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'agendar' },
      {
        path: 'agendar',
        loadComponent: () => import('./features/professor/agendar/agendar').then((m) => m.Agendar)
      },
      {
        path: 'meus-agendamentos',
        loadComponent: () =>
          import('./features/professor/meus-agendamentos/meus-agendamentos').then(
            (m) => m.MeusAgendamentos
          )
      },
      {
        path: 'convites/:id',
        loadComponent: () =>
          import('./features/professor/convite-detalhe/convite-detalhe').then(
            (m) => m.ConviteDetalhe
          )
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
