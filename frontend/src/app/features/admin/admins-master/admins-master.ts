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

  iniciais(nome: string): string {
    const partes = nome.trim().split(/\s+/).filter(Boolean);
    if (partes.length === 0) return '?';
    const primeira = partes[0][0];
    const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
    return (primeira + ultima).toUpperCase();
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

  async salvarAdmin(): Promise<void> {
    this.erro.set(null);

    const editandoId = this.editandoId();
    if (editandoId) {
      const confirmado = await this.confirmDialog.confirmar({
        titulo: 'Salvar alterações',
        mensagem: `Salvar as alterações do administrador "${this.novoNome}"?`,
        textoConfirmar: 'Salvar',
        aoConfirmar: () => this.adminService.atualizar(editandoId, { nome: this.novoNome, email: this.novoEmail })
      });
      if (confirmado) {
        this.fecharFormulario();
        this.carregar();
      }
      return;
    }

    this.salvando.set(true);
    this.adminService
      .criar({ nome: this.novoNome, email: this.novoEmail, senha: this.novaSenha })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.fecharFormulario();
          this.carregar();
        },
        error: (err: { error?: { message?: string } }) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível salvar o administrador.');
        }
      });
  }

  async excluir(admin: Admin): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir administrador',
      mensagem: `Excluir o administrador "${admin.nome}"? Essa ação não pode ser desfeita.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo',
      aoConfirmar: () => this.adminService.excluir(admin.id)
    });
    if (confirmado) {
      this.carregar();
    }
  }
}
