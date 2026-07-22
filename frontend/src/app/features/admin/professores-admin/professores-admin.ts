import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/professor.models';
import { Icon } from '../../../shared/icon/icon';

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
  readonly criando = signal(false);

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

  constructor(private readonly professorService: ProfessorService) {}

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

  toggleFormulario(): void {
    this.formularioAberto.update((aberto) => !aberto);
    this.erro.set(null);
  }

  criarProfessor(): void {
    this.erro.set(null);
    this.criando.set(true);

    this.professorService
      .criar({ nome: this.novoNome, email: this.novoEmail, senha: this.novaSenha, cpf: this.novoCpf })
      .subscribe({
        next: () => {
          this.criando.set(false);
          this.novoNome = '';
          this.novoEmail = '';
          this.novaSenha = '';
          this.novoCpf = '';
          this.formularioAberto.set(false);
          this.carregar();
        },
        error: (err) => {
          this.criando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível cadastrar o professor.');
        }
      });
  }
}
