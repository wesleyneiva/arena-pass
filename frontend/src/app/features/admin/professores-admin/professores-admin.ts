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
  readonly excluindoId = signal<string | null>(null);

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

  aprovar(id: string): void {
    this.professorService.aprovar(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível aprovar.')
    });
  }

  suspender(id: string): void {
    this.professorService.suspender(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível suspender.')
    });
  }

  reativar(id: string): void {
    this.professorService.reativar(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível reativar.')
    });
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

  salvarProfessor(): void {
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
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar o professor.');
      }
    };

    if (editandoId) {
      this.professorService
        .atualizar(editandoId, { nome: this.novoNome, email: this.novoEmail, cpf: this.novoCpf })
        .subscribe(aoConcluir);
    } else {
      this.professorService
        .criar({ nome: this.novoNome, email: this.novoEmail, senha: this.novaSenha, cpf: this.novoCpf })
        .subscribe(aoConcluir);
    }
  }

  async excluir(professor: Professor): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir professor',
      mensagem: `Excluir o professor "${professor.nome}"? Essa ação não pode ser desfeita.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo'
    });
    if (!confirmado) {
      return;
    }

    this.erro.set(null);
    this.excluindoId.set(professor.id);

    this.professorService.excluir(professor.id).subscribe({
      next: () => {
        this.excluindoId.set(null);
        this.carregar();
      },
      error: (err) => {
        this.excluindoId.set(null);
        this.erro.set(err?.error?.message ?? 'Não foi possível excluir o professor.');
      }
    });
  }
}
