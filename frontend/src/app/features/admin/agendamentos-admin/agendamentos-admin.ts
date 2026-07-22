import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { Agendamento, FormaPagamento } from '../../../core/models/agendamento.models';

type FiltroStatus = 'Todos' | 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';

@Component({
  selector: 'app-agendamentos-admin',
  imports: [FormsModule],
  templateUrl: './agendamentos-admin.html'
})
export class AgendamentosAdmin implements OnInit {
  readonly agendamentos = signal<Agendamento[]>([]);
  readonly erro = signal<string | null>(null);

  readonly filtroStatus = signal<FiltroStatus>('Todos');
  readonly filtroProfessor = signal('Todos');
  readonly filtroDataDe = signal('');
  readonly filtroDataAte = signal('');

  formaPagamentoPorAgendamento: Record<string, FormaPagamento> = {};

  readonly professoresDisponiveis = computed(() => {
    const nomes = new Set(this.agendamentos().map((a) => a.professorNome));
    return ['Todos', ...Array.from(nomes).sort()];
  });

  readonly agendamentosFiltrados = computed(() => {
    const status = this.filtroStatus();
    const professor = this.filtroProfessor();
    const dataDe = this.filtroDataDe();
    const dataAte = this.filtroDataAte();

    return this.agendamentos().filter((a) => {
      const bateStatus = status === 'Todos' || a.status === status;
      const bateProfessor = professor === 'Todos' || a.professorNome === professor;
      const bateDataDe = dataDe === '' || a.data >= dataDe;
      const bateDataAte = dataAte === '' || a.data <= dataAte;
      return bateStatus && bateProfessor && bateDataDe && bateDataAte;
    });
  });

  constructor(private readonly agendamentoService: AgendamentoService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.agendamentoService.listarTodos().subscribe((agendamentos) => {
      this.agendamentos.set(agendamentos);
      for (const agendamento of agendamentos) {
        this.formaPagamentoPorAgendamento[agendamento.id] ??= 'Pix';
      }
    });
  }

  confirmarPagamento(id: string): void {
    this.erro.set(null);
    const formaPagamento = this.formaPagamentoPorAgendamento[id] ?? 'Pix';
    this.agendamentoService.confirmarPagamento(id, formaPagamento).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível confirmar o pagamento.')
    });
  }

  marcarRealizado(id: string): void {
    this.agendamentoService.marcarRealizado(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível marcar como realizada.')
    });
  }

  cancelar(id: string): void {
    if (!confirm('Cancelar esse agendamento?')) {
      return;
    }
    this.agendamentoService.cancelar(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível cancelar.')
    });
  }
}
