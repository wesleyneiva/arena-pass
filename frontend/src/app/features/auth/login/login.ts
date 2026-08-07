import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { EspacoService } from '../../../core/services/espaco.service';
import { TenantService } from '../../../core/services/tenant.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html'
})
export class Login implements OnInit {
  email = '';
  senha = '';
  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly anoAtual = new Date().getFullYear();

  readonly dominioMaster: boolean;

  // Resolução do espaço é só informativa aqui — não bloqueia o formulário, porque o
  // Master loga sem nenhum espaço resolvido (é cross-tenant). Quem de fato valida o
  // vínculo com o espaço é o próprio POST /auth/login no backend.
  readonly nomeEspaco = signal<string | null>(null);
  readonly espacoNaoEncontrado = signal(false);

  constructor(
    private readonly auth: AuthService,
    private readonly espacos: EspacoService,
    private readonly tenant: TenantService,
    private readonly router: Router
  ) {
    this.dominioMaster = this.tenant.ehDominioMaster();
  }

  ngOnInit(): void {
    // arenapass.wnlabs.com.br é o domínio exclusivo do Master — nunca tem espaço pra
    // resolver, então nem vale a pena chamar o endpoint.
    if (this.dominioMaster) {
      return;
    }

    this.espacos.resolverAtual().subscribe({
      next: (resultado) => {
        this.nomeEspaco.set(resultado.nome);
        this.espacoNaoEncontrado.set(!resultado.encontrado);
      },
      error: () => this.espacoNaoEncontrado.set(true)
    });
  }

  entrar(): void {
    this.erro.set(null);
    this.carregando.set(true);

    this.auth.login({ email: this.email, senha: this.senha }).subscribe({
      next: (resultado) => {
        this.carregando.set(false);
        if (resultado.role === 'Master') {
          this.router.navigateByUrl('/admin/dashboard');
        } else if (resultado.role === 'AdminClube') {
          this.router.navigateByUrl('/admin/quadras');
        } else {
          this.router.navigateByUrl('/professor/agendar');
        }
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível entrar. Verifique seus dados.');
      }
    });
  }
}
