import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EspacoService } from '../../../core/services/espaco.service';
import { AdminService } from '../../../core/services/admin.service';
import { Espaco } from '../../../core/models/espaco.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-espacos-master',
  imports: [FormsModule, Icon],
  templateUrl: './espacos-master.html'
})
export class EspacosMaster implements OnInit {
  readonly espacos = signal<Espaco[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);

  readonly formularioEspacoAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly salvandoEspaco = signal(false);
  novoNomeEspaco = '';
  novoSubdominio = '';

  readonly espacoParaAdmin = signal<Espaco | null>(null);
  readonly salvandoAdmin = signal(false);
  readonly erroAdmin = signal<string | null>(null);
  novoNomeAdmin = '';
  novoEmailAdmin = '';
  novaSenhaAdmin = '';

  constructor(
    private readonly espacoService: EspacoService,
    private readonly adminService: AdminService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.espacoService.listar().subscribe({
      next: (espacos) => {
        this.espacos.set(espacos);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os espaços.');
      }
    });
  }

  abrirNovoEspaco(): void {
    this.editandoId.set(null);
    this.novoNomeEspaco = '';
    this.novoSubdominio = '';
    this.erro.set(null);
    this.formularioEspacoAberto.set(true);
  }

  editarEspaco(espaco: Espaco): void {
    this.editandoId.set(espaco.id);
    this.novoNomeEspaco = espaco.nome;
    this.novoSubdominio = espaco.subdominio;
    this.erro.set(null);
    this.formularioEspacoAberto.set(true);
  }

  fecharNovoEspaco(): void {
    this.formularioEspacoAberto.set(false);
    this.editandoId.set(null);
  }

  sugerirSubdominio(): void {
    if (this.novoSubdominio || this.editandoId()) {
      return;
    }
    this.novoSubdominio = this.novoNomeEspaco
      .toLowerCase()
      .normalize('NFD')
      .replace(new RegExp('[\\u0300-\\u036f]', 'g'), '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  }

  salvarEspaco(): void {
    this.erro.set(null);
    this.salvandoEspaco.set(true);

    const aoConcluir = {
      next: () => {
        this.salvandoEspaco.set(false);
        this.fecharNovoEspaco();
        this.carregar();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvandoEspaco.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar o espaço.');
      }
    };

    const editandoId = this.editandoId();
    const dados = { nome: this.novoNomeEspaco, subdominio: this.novoSubdominio };

    if (editandoId) {
      this.espacoService.atualizar(editandoId, dados).subscribe(aoConcluir);
    } else {
      this.espacoService.criar(dados).subscribe(aoConcluir);
    }
  }

  async alternarStatus(espaco: Espaco): Promise<void> {
    const vaiBloquear = espaco.ativo;
    const confirmado = await this.confirmDialog.confirmar({
      titulo: vaiBloquear ? 'Bloquear espaço' : 'Reativar espaço',
      mensagem: vaiBloquear
        ? `Bloquear "${espaco.nome}"? O acesso de admin e professores desse espaço é cortado na hora — use em caso de inadimplência.`
        : `Reativar "${espaco.nome}"? Admin e professores voltam a acessar normalmente.`,
      textoConfirmar: vaiBloquear ? 'Bloquear' : 'Reativar',
      variante: vaiBloquear ? 'perigo' : 'padrao',
      aoConfirmar: () => this.espacoService.atualizarStatus(espaco.id, !vaiBloquear)
    });

    if (confirmado) {
      this.carregar();
    }
  }

  async excluirEspaco(espaco: Espaco): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir espaço',
      mensagem: `Excluir "${espaco.nome}"? Só é possível se ele ainda não tiver admin, quadra ou professor vinculado.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo',
      aoConfirmar: () => this.espacoService.excluir(espaco.id)
    });

    if (confirmado) {
      this.carregar();
    }
  }

  abrirCriarAdmin(espaco: Espaco): void {
    this.novoNomeAdmin = '';
    this.novoEmailAdmin = '';
    this.novaSenhaAdmin = '';
    this.erroAdmin.set(null);
    this.espacoParaAdmin.set(espaco);
  }

  fecharCriarAdmin(): void {
    this.espacoParaAdmin.set(null);
  }

  salvarAdmin(): void {
    const espaco = this.espacoParaAdmin();
    if (!espaco) {
      return;
    }

    this.erroAdmin.set(null);
    this.salvandoAdmin.set(true);
    this.adminService
      .criar({
        nome: this.novoNomeAdmin,
        email: this.novoEmailAdmin,
        senha: this.novaSenhaAdmin,
        espacoId: espaco.id
      })
      .subscribe({
        next: () => {
          this.salvandoAdmin.set(false);
          this.fecharCriarAdmin();
        },
        error: (err) => {
          this.salvandoAdmin.set(false);
          this.erroAdmin.set(err?.error?.message ?? 'Não foi possível criar o administrador.');
        }
      });
  }
}
