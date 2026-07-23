import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html'
})
export class Login {
  email = '';
  senha = '';
  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly anoAtual = new Date().getFullYear();

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  entrar(): void {
    this.erro.set(null);
    this.carregando.set(true);

    this.auth.login({ email: this.email, senha: this.senha }).subscribe({
      next: (resultado) => {
        this.carregando.set(false);
        if (resultado.role === 'AdminClube' || resultado.role === 'Master') {
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
