import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/professor.models';

type FiltroStatus = 'Todos' | 'Pendente' | 'Aprovado' | 'Suspenso';

@Component({
  selector: 'app-professores-admin',
  imports: [FormsModule],
  templateUrl: './professores-admin.html'
})
export class ProfessoresAdmin implements OnInit {
  readonly professores = signal<Professor[]>([]);
  readonly busca = signal('');
  readonly filtroStatus = signal<FiltroStatus>('Todos');

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
    this.professorService.listar().subscribe((professores) => this.professores.set(professores));
  }

  atualizarBusca(valor: string): void {
    this.busca.set(valor);
  }

  aprovar(id: string): void {
    this.professorService.aprovar(id).subscribe(() => this.carregar());
  }
}
