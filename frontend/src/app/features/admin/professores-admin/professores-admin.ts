import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/professor.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

type FiltroStatus = 'Todos' | 'Pendente' | 'Aprovado' | 'Suspenso';

@Component({
  selector: 'app-professores-admin',
  imports: [FormsModule, Icon],
  templateUrl: './professores-admin.html'
})
export class ProfessoresAdmin implements OnInit {
  readonly professores = signal<Professor[]>([]);
  readonly busca = signal('');
  readonly filtroStatus = signal<FiltroStatus>('Todos');
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);
  readonly formularioAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly salvando = signal(false);

  novoNome = '';
  novoEmail = '';
  novaSenha = '';
  novoCpf = '';

  readonly professoresFiltrados = computed(() => {
    const busca = this.busca().trim().toLowerCase();
    const status = this.filtroStatus();

    return this.professores().filter((p) => {
      const bateStatus = status === 'Todos' || p.statusAprovacao === status;
      const bateBusca =
        busca === '' ||
        p.nome.toLowerCase().includes(busca) ||
        p.email.toLowerCase().includes(busca) ||
        p.cpf.includes(busca);
      return bateStatus && bateBusca;
    });
  });

  constructor(
    private readonly professorService: ProfessorService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.professorService.listar().subscribe({
      next: (professores) => {
        this.professores.set(professores);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os professores.');
      }
    });
  }

  atualizarBusca(valor: string): void {
    this.busca.set(valor);
  }

  iniciais(nome: string): string {
    const partes = nome.trim().split(/\s+/).filter(Boolean);
    if (partes.length === 0) return '?';
    const primeira = partes[0][0];
    const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
    return (primeira + ultima).toUpperCase();
  }

  corStatus(status: Professor['statusAprovacao']): { badge: string; barra: string } {
    switch (status) {
      case 'Pendente':
        return { badge: 'bg-amber-50 text-amber-700', barra: 'bg-amber-400' };
      case 'Aprovado':
        return { badge: 'bg-blue-50 text-blue-600', barra: 'bg-blue-500' };
      case 'Suspenso':
        return { badge: 'bg-red-50 text-red-700', barra: 'bg-red-300' };
    }
  }

  async aprovar(professor: Professor): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Aprovar professor',
      mensagem: `Aprovar o professor "${professor.nome}"? Ele passará a ter acesso normal ao sistema.`,
      textoConfirmar: 'Aprovar',
      aoConfirmar: () => this.professorService.aprovar(professor.id)
    });
    if (confirmado) {
      this.carregar();
    }
  }

  async suspender(professor: Professor): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Suspender professor',
      mensagem: `Suspender o professor "${professor.nome}"? Ele perderá o acesso ao sistema até ser reativado.`,
      textoConfirmar: 'Suspender',
      variante: 'perigo',
      aoConfirmar: () => this.professorService.suspender(professor.id)
    });
    if (confirmado) {
      this.carregar();
    }
  }

  async reativar(professor: Professor): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Reativar professor',
      mensagem: `Reativar o professor "${professor.nome}"? Ele voltará a ter acesso ao sistema.`,
      textoConfirmar: 'Reativar',
      aoConfirmar: () => this.professorService.reativar(professor.id)
    });
    if (confirmado) {
      this.carregar();
    }
  }

  abrirNovo(): void {
    this.editandoId.set(null);
    this.novoNome = '';
    this.novoEmail = '';
    this.novaSenha = '';
    this.novoCpf = '';
    this.formularioAberto.set(true);
    this.erro.set(null);
  }

  editar(professor: Professor): void {
    this.editandoId.set(professor.id);
    this.novoNome = professor.nome;
    this.novoEmail = professor.email;
    this.novoCpf = professor.cpf;
    this.novaSenha = '';
    this.formularioAberto.set(true);
    this.erro.set(null);
  }

  fecharFormulario(): void {
    this.formularioAberto.set(false);
    this.editandoId.set(null);
  }

  async salvarProfessor(): Promise<void> {
    this.erro.set(null);

    const editandoId = this.editandoId();
    if (editandoId) {
      const confirmado = await this.confirmDialog.confirmar({
        titulo: 'Salvar alterações',
        mensagem: `Salvar as alterações do professor "${this.novoNome}"?`,
        textoConfirmar: 'Salvar',
        aoConfirmar: () =>
          this.professorService.atualizar(editandoId, { nome: this.novoNome, email: this.novoEmail, cpf: this.novoCpf })
      });
      if (confirmado) {
        this.fecharFormulario();
        this.carregar();
      }
      return;
    }

    this.salvando.set(true);
    this.professorService
      .criar({ nome: this.novoNome, email: this.novoEmail, senha: this.novaSenha, cpf: this.novoCpf })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.fecharFormulario();
          this.carregar();
        },
        error: (err: { error?: { message?: string } }) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível salvar o professor.');
        }
      });
  }

  async excluir(professor: Professor): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir professor',
      mensagem: `Excluir o professor "${professor.nome}"? Essa ação não pode ser desfeita.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo',
      aoConfirmar: () => this.professorService.excluir(professor.id)
    });
    if (confirmado) {
      this.carregar();
    }
  }
}
