import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { ConviteService } from '../../../core/services/convite.service';
import { NotificacaoService } from '../../../core/services/notificacao.service';
import { Agendamento, FormaPagamento } from '../../../core/models/agendamento.models';
import { ConviteResumo } from '../../../core/models/convite.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { DataBrPipe } from '../../../shared/pipes/data-br.pipe';
import { Paginador } from '../../../shared/paginador/paginador';

type FiltroStatus = 'Todos' | 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';

const ITENS_POR_PAGINA = 10;

@Component({
  selector: 'app-agendamentos-admin',
  imports: [FormsModule, DataBrPipe, Paginador],
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

  readonly agendamentoExpandidoId = signal<string | null>(null);
  readonly convitesPorAgendamento = signal<Record<string, ConviteResumo[]>>({});

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
      const bateStatus = status === 'Todos' || this.statusEfetivo(a) === status;
      const bateProfessor = professor === 'Todos' || a.professorNome === professor;
      const bateDataDe = dataDe === '' || a.data >= dataDe;
      const bateDataAte = dataAte === '' || a.data <= dataAte;
      return bateStatus && bateProfessor && bateDataDe && bateDataAte;
    });
  });

  private statusEfetivo(agendamento: Agendamento): FiltroStatus {
    if (agendamento.encerrado && agendamento.status === 'Confirmado') {
      return 'Realizado';
    }
    return agendamento.status as FiltroStatus;
  }

  readonly paginaAtual = signal(1);

  readonly totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.agendamentosFiltrados().length / ITENS_POR_PAGINA))
  );

  readonly paginaEfetiva = computed(() => Math.min(this.paginaAtual(), this.totalPaginas()));

  readonly agendamentosPaginados = computed(() => {
    const inicio = (this.paginaEfetiva() - 1) * ITENS_POR_PAGINA;
    return this.agendamentosFiltrados().slice(inicio, inicio + ITENS_POR_PAGINA);
  });

  constructor(
    private readonly agendamentoService: AgendamentoService,
    private readonly conviteService: ConviteService,
    private readonly confirmDialog: ConfirmDialogService,
    private readonly notificacaoService: NotificacaoService
  ) {}

  ngOnInit(): void {
    this.carregar();
    // Abrir a página de agendamentos "vê" as novidades — zera o badge da navbar.
    this.notificacaoService.marcarTodasLidas().subscribe({ error: () => {} });
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
      textoConfirmar: 'Confirmar pagamento',
      aoConfirmar: () => this.agendamentoService.confirmarPagamento(id, formaPagamento)
    });
    if (confirmado) {
      this.carregar();
    }
  }

  convitesDe(agendamentoId: string): ConviteResumo[] {
    return this.convitesPorAgendamento()[agendamentoId] ?? [];
  }

  toggleConvites(agendamentoId: string): void {
    if (this.agendamentoExpandidoId() === agendamentoId) {
      this.agendamentoExpandidoId.set(null);
      return;
    }

    this.agendamentoExpandidoId.set(agendamentoId);
    this.conviteService.listarPorAgendamento(agendamentoId).subscribe((convites) => {
      this.convitesPorAgendamento.update((atual) => ({ ...atual, [agendamentoId]: convites }));
    });
  }

  rotuloStatus(agendamento: Agendamento): string {
    if (agendamento.encerrado && agendamento.status === 'Confirmado') {
      return 'Realizada';
    }
    if (agendamento.encerrado && agendamento.status === 'PendentePagamento') {
      return 'Expirado';
    }
    return agendamento.status;
  }

  corStatus(agendamento: Agendamento): { badge: string; barra: string } {
    switch (this.rotuloStatus(agendamento)) {
      case 'Confirmado':
        return { badge: 'bg-blue-50 text-blue-700', barra: 'bg-blue-500' };
      case 'PendentePagamento':
        return { badge: 'bg-amber-50 text-amber-700', barra: 'bg-amber-400' };
      case 'Realizada':
        return { badge: 'bg-emerald-50 text-emerald-700', barra: 'bg-emerald-500' };
      case 'Cancelado':
        return { badge: 'bg-red-50 text-red-700', barra: 'bg-red-300' };
      case 'Expirado':
        return { badge: 'bg-slate-100 text-slate-500', barra: 'bg-slate-300' };
      default:
        return { badge: 'bg-slate-100 text-slate-700', barra: 'bg-slate-300' };
    }
  }

  async cancelar(id: string): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Cancelar agendamento',
      mensagem: 'Tem certeza que deseja cancelar esse agendamento? Essa ação não pode ser desfeita.',
      textoConfirmar: 'Cancelar agendamento',
      textoCancelar: 'Voltar',
      variante: 'perigo',
      aoConfirmar: () => this.agendamentoService.cancelar(id)
    });
    if (confirmado) {
      this.carregar();
    }
  }
}
