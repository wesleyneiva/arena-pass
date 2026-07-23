import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-perfil-admin',
  imports: [FormsModule],
  templateUrl: './perfil-admin.html'
})
export class PerfilAdmin {
  readonly erro = signal<string | null>(null);
  readonly sucesso = signal<string | null>(null);
  readonly salvando = signal(false);

  email: string;
  senhaAtual = '';
  novaSenha = '';
  confirmarNovaSenha = '';

  constructor(private readonly auth: AuthService) {
    this.email = this.auth.usuario()?.email ?? '';
  }

  salvar(): void {
    this.erro.set(null);
    this.sucesso.set(null);

    if (this.novaSenha && this.novaSenha !== this.confirmarNovaSenha) {
      this.erro.set('A confirmação da nova senha não confere.');
      return;
    }

    this.salvando.set(true);
    this.auth
      .atualizarPerfil({
        email: this.email,
        senhaAtual: this.senhaAtual,
        novaSenha: this.novaSenha || undefined
      })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.sucesso.set('Perfil atualizado com sucesso.');
          this.senhaAtual = '';
          this.novaSenha = '';
          this.confirmarNovaSenha = '';
        },
        error: (err) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível atualizar o perfil.');
        }
      });
  }
}
