import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { EspacoService } from '../../../core/services/espaco.service';
import { TenantService } from '../../../core/services/tenant.service';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

type Etapa = 'dados' | 'codigo';

@Component({
  selector: 'app-registrar',
  imports: [FormsModule, RouterLink],
  templateUrl: './registrar.html'
})
export class Registrar implements OnInit {
  readonly etapa = signal<Etapa>('dados');

  nome = '';
  email = '';
  senha = '';
  cpf = '';
  codigo = '';

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly nomeEspaco = signal<string | null>(null);
  readonly espacoNaoEncontrado = signal(false);
  readonly anoAtual = new Date().getFullYear();
  readonly dominioMaster: boolean;

  constructor(
    private readonly auth: AuthService,
    private readonly espacos: EspacoService,
    private readonly tenant: TenantService,
    private readonly router: Router,
    private readonly confirmDialog: ConfirmDialogService
  ) {
    this.dominioMaster = this.tenant.ehDominioMaster();
  }

  ngOnInit(): void {
    // arenapass.wnlabs.com.br é exclusivo do Master — nunca tem espaço pra resolver
    // nem faz sentido um professor se cadastrar por lá.
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
