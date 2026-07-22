import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-registrar',
  imports: [FormsModule, RouterLink],
  templateUrl: './registrar.html'
})
export class Registrar {
  nome = '';
  email = '';
  senha = '';
  cpf = '';

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  registrar(): void {
    this.erro.set(null);
    this.carregando.set(true);

    this.auth
      .registrarProfessor({ nome: this.nome, email: this.email, senha: this.senha, cpf: this.cpf })
      .subscribe({
        next: () => {
          this.carregando.set(false);
          this.router.navigateByUrl('/login');
        },
        error: (err) => {
          this.carregando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível concluir o cadastro.');
        }
      });
  }
}
