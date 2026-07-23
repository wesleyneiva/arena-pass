import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

type Etapa = 'dados' | 'codigo';

@Component({
  selector: 'app-registrar',
  imports: [FormsModule, RouterLink],
  templateUrl: './registrar.html'
})
export class Registrar {
  readonly etapa = signal<Etapa>('dados');

  nome = '';
  email = '';
  senha = '';
  cpf = '';
  codigo = '';

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  solicitarCodigo(): void {
    this.erro.set(null);
    this.carregando.set(true);

    this.auth
      .solicitarCodigoRegistroProfessor({ nome: this.nome, email: this.email, senha: this.senha, cpf: this.cpf })
      .subscribe({
        next: () => {
          this.carregando.set(false);
          this.etapa.set('codigo');
        },
        error: (err) => {
          this.carregando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível enviar o código. Verifique os dados.');
        }
      });
  }

  confirmarCodigo(): void {
    this.erro.set(null);
    this.carregando.set(true);

    this.auth.confirmarCodigoRegistroProfessor({ email: this.email, codigo: this.codigo }).subscribe({
      next: async () => {
        this.carregando.set(false);
        await this.confirmDialog.confirmar({
          titulo: 'Cadastro criado com sucesso!',
          mensagem: 'Aguarde a aprovação do clube para fazer as marcações de aulas.',
          textoConfirmar: 'Entendi',
          somenteConfirmar: true
        });
        this.router.navigateByUrl('/login');
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Código inválido. Tente novamente.');
      }
    });
  }

  voltarParaDados(): void {
    this.erro.set(null);
    this.codigo = '';
    this.etapa.set('dados');
  }
}
