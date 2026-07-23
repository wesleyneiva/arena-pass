import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';
import { Admin } from '../../../core/models/admin.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-admins-master',
  imports: [FormsModule, Icon],
  templateUrl: './admins-master.html'
})
export class AdminsMaster implements OnInit {
  readonly admins = signal<Admin[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);
  readonly formularioAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly salvando = signal(false);
  readonly excluindoId = signal<string | null>(null);

  novoNome = '';
  novoEmail = '';
  novaSenha = '';

  constructor(
    private readonly adminService: AdminService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.adminService.listar().subscribe({
      next: (admins) => {
        this.admins.set(admins);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os administradores.');
      }
    });
  }

  abrirNovo(): void {
    this.editandoId.set(null);
    this.novoNome = '';
    this.novoEmail = '';
    this.novaSenha = '';
    this.formularioAberto.set(true);
    this.erro.set(null);
  }

  editar(admin: Admin): void {
    this.editandoId.set(admin.id);
    this.novoNome = admin.nome;
    this.novoEmail = admin.email;
    this.novaSenha = '';
    this.formularioAberto.set(true);
    this.erro.set(null);
  }

  fecharFormulario(): void {
    this.formularioAberto.set(false);
    this.editandoId.set(null);
  }

  salvarAdmin(): void {
    this.erro.set(null);
    this.salvando.set(true);

    const editandoId = this.editandoId();
    const aoConcluir = {
      next: () => {
        this.salvando.set(false);
        this.fecharFormulario();
        this.carregar();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar o administrador.');
      }
    };

    if (editandoId) {
      this.adminService.atualizar(editandoId, { nome: this.novoNome, email: this.novoEmail }).subscribe(aoConcluir);
    } else {
      this.adminService
        .criar({ nome: this.novoNome, email: this.novoEmail, senha: this.novaSenha })
        .subscribe(aoConcluir);
    }
  }

  async excluir(admin: Admin): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir administrador',
      mensagem: `Excluir o administrador "${admin.nome}"? Essa ação não pode ser desfeita.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo'
    });
    if (!confirmado) {
      return;
    }

    this.erro.set(null);
    this.excluindoId.set(admin.id);

    this.adminService.excluir(admin.id).subscribe({
      next: () => {
        this.excluindoId.set(null);
        this.carregar();
      },
      error: (err) => {
        this.excluindoId.set(null);
        this.erro.set(err?.error?.message ?? 'Não foi possível excluir o administrador.');
      }
    });
  }
}
