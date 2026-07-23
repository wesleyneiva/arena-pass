import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { Agendamento, FormaPagamento } from '../../../core/models/agendamento.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { DataBrPipe } from '../../../shared/pipes/data-br.pipe';

type FiltroStatus = 'Todos' | 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';

@Component({
  selector: 'app-agendamentos-admin',
  imports: [FormsModule, DataBrPipe],
  templateUrl: './agendamentos-admin.html'
})
export class AgendamentosAdmin implements OnInit {
  readonly agendamentos = signal<Agendamento[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);

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

  constructor(
    private readonly agendamentoService: AgendamentoService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.agendamentoService.listarTodos().subscribe({
      next: (agendamentos) => {
        this.agendamentos.set(agendamentos);
        for (const agendamento of agendamentos) {
          this.formaPagamentoPorAgendamento[agendamento.id] ??= 'Pix';
        }
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os agendamentos.');
      }
    });
  }

  async confirmarPagamento(id: string): Promise<void> {
    const formaPagamento = this.formaPagamentoPorAgendamento[id] ?? 'Pix';
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Confirmar pagamento',
      mensagem: `Confirmar o recebimento do pagamento via ${formaPagamento === 'Cartao' ? 'cartão' : formaPagamento.toLowerCase()}?`,
      textoConfirmar: 'Confirmar pagamento'
    });
    if (!confirmado) {
      return;
    }

    this.erro.set(null);
    this.agendamentoService.confirmarPagamento(id, formaPagamento).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível confirmar o pagamento.')
    });
  }

  async marcarRealizado(id: string): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Marcar como realizada',
      mensagem: 'Confirma que essa aula foi realizada?',
      textoConfirmar: 'Marcar como realizada'
    });
    if (!confirmado) {
      return;
    }

    this.agendamentoService.marcarRealizado(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível marcar como realizada.')
    });
  }

  async cancelar(id: string): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Cancelar agendamento',
      mensagem: 'Tem certeza que deseja cancelar esse agendamento? Essa ação não pode ser desfeita.',
      textoConfirmar: 'Cancelar agendamento',
      textoCancelar: 'Voltar',
      variante: 'perigo'
    });
    if (!confirmado) {
      return;
    }

    this.agendamentoService.cancelar(id).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível cancelar.')
    });
  }
}
