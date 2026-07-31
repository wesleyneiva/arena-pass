import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { ConviteService } from '../../../core/services/convite.service';
import { Agendamento, FormaPagamento, PagamentoPix } from '../../../core/models/agendamento.models';
import { ConviteResumo } from '../../../core/models/convite.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { DataBrPipe } from '../../../shared/pipes/data-br.pipe';
import { Paginador } from '../../../shared/paginador/paginador';

type FiltroStatus = 'Todos' | 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';

const ITENS_POR_PAGINA = 10;

@Component({
  selector: 'app-meus-agendamentos',
  imports: [FormsModule, RouterLink, DataBrPipe, Paginador],
  templateUrl: './meus-agendamentos.html'
})
export class MeusAgendamentos implements OnInit {
  readonly agendamentos = signal<Agendamento[]>([]);
  readonly agendamentoExpandidoId = signal<string | null>(null);
  readonly convitesPorAgendamento = signal<Record<string, ConviteResumo[]>>({});
  readonly erro = signal<string | null>(null);
  readonly emitindo = signal(false);
  readonly carregando = signal(true);

  readonly filtroStatus = signal<FiltroStatus>('Todos');
  readonly filtroQuadra = signal('Todas');
  readonly filtroDataDe = signal('');
  readonly filtroDataAte = signal('');
  readonly busca = signal('');

  readonly modalPagamentoId = signal<string | null>(null);
  readonly formaPagamentoEscolhida = signal<FormaPagamento>('Pix');
  readonly pagamentoPix = signal<PagamentoPix | null>(null);
  readonly carregandoPix = signal(false);
  readonly confirmandoPagamento = signal(false);
  readonly copiado = signal(false);
  readonly erroPagamento = signal<string | null>(null);

  readonly sucessoPagamentoInfo = signal<{ quadraNome: string; data: string; horaInicio: string; formaPagamento: FormaPagamento } | null>(null);

  alunoNome = '';
  alunoCpf = '';

  readonly quadrasDisponiveis = computed(() => {
    const nomes = new Set(this.agendamentos().map((a) => a.quadraNome));
    return ['Todas', ...Array.from(nomes).sort()];
  });

  readonly agendamentosFiltrados = computed(() => {
    const status = this.filtroStatus();
    const quadra = this.filtroQuadra();
    const dataDe = this.filtroDataDe();
    const dataAte = this.filtroDataAte();
    const busca = this.busca().trim().toLowerCase();

    return this.agendamentos().filter((a) => {
      const bateStatus = status === 'Todos' || this.statusEfetivo(a) === status;
      const bateQuadra = quadra === 'Todas' || a.quadraNome === quadra;
      const bateDataDe = dataDe === '' || a.data >= dataDe;
      const bateDataAte = dataAte === '' || a.data <= dataAte;
      const bateBusca = busca === '' || a.quadraNome.toLowerCase().includes(busca);
      return bateStatus && bateQuadra && bateDataDe && bateDataAte && bateBusca;
    });
  });

  private statusEfetivo(agendamento: Agendamento): FiltroStatus {
    if (agendamento.encerrado && agendamento.status === 'Confirmado') {
      return 'Realizado';
    }
    return agendamento.status as FiltroStatus;
  }

  readonly agendamentoEmPagamento = computed(() =>
    this.agendamentos().find((a) => a.id === this.modalPagamentoId()) ?? null
  );

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
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.agendamentoService.meus().subscribe({
      next: (agendamentos) => {
        this.agendamentos.set(agendamentos);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar seus agendamentos.');
      }
    });
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
    this.alunoNome = '';
    this.alunoCpf = '';
    this.erro.set(null);

    this.conviteService.listarPorAgendamento(agendamentoId).subscribe((convites) => {
      this.convitesPorAgendamento.update((atual) => ({ ...atual, [agendamentoId]: convites }));
    });
  }

  emitirConvite(agendamentoId: string): void {
    this.erro.set(null);
    this.emitindo.set(true);

    this.conviteService.emitir(agendamentoId, { alunoNome: this.alunoNome, alunoCpf: this.alunoCpf }).subscribe({
      next: () => {
        this.emitindo.set(false);
        this.alunoNome = '';
        this.alunoCpf = '';
        this.conviteService.listarPorAgendamento(agendamentoId).subscribe((convites) => {
          this.convitesPorAgendamento.update((atual) => ({ ...atual, [agendamentoId]: convites }));
        });
      },
      error: (err) => {
        this.emitindo.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível emitir o convite.');
      }
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

  async cancelar(agendamentoId: string): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Cancelar agendamento',
      mensagem: 'Tem certeza que deseja cancelar esse agendamento? Essa ação não pode ser desfeita.',
      textoConfirmar: 'Cancelar agendamento',
      textoCancelar: 'Voltar',
      variante: 'perigo',
      aoConfirmar: () => this.agendamentoService.cancelar(agendamentoId)
    });
    if (confirmado) {
      this.carregar();
    }
  }

  abrirPagamento(agendamentoId: string): void {
    this.modalPagamentoId.set(agendamentoId);
    this.formaPagamentoEscolhida.set('Pix');
    this.pagamentoPix.set(null);
    this.erroPagamento.set(null);
    this.copiado.set(false);
    this.carregarPix(agendamentoId);
  }

  fecharPagamento(): void {
    this.modalPagamentoId.set(null);
    this.pagamentoPix.set(null);
  }

  escolherFormaPagamento(forma: FormaPagamento): void {
    this.formaPagamentoEscolhida.set(forma);
    this.copiado.set(false);

    const agendamentoId = this.modalPagamentoId();
    if (forma === 'Pix' && agendamentoId && !this.pagamentoPix()) {
      this.carregarPix(agendamentoId);
    }
  }

  private carregarPix(agendamentoId: string): void {
    this.carregandoPix.set(true);
    this.erroPagamento.set(null);

    this.agendamentoService.obterPagamentoPix(agendamentoId).subscribe({
      next: (pagamento) => {
        this.pagamentoPix.set(pagamento);
        this.carregandoPix.set(false);
      },
      error: (err) => {
        this.carregandoPix.set(false);
        this.erroPagamento.set(err?.error?.message ?? 'Não foi possível gerar o QR Code Pix.');
      }
    });
  }

  copiarPixCopiaECola(): void {
    const pagamento = this.pagamentoPix();
    if (!pagamento) {
      return;
    }

    navigator.clipboard
      .writeText(pagamento.pixCopiaECola)
      .then(() => {
        this.copiado.set(true);
        setTimeout(() => this.copiado.set(false), 2000);
      })
      .catch(() => this.erroPagamento.set('Não foi possível copiar. Selecione e copie o texto manualmente.'));
  }

  confirmarPagamentoModal(): void {
    const agendamentoId = this.modalPagamentoId();
    const agendamento = this.agendamentoEmPagamento();
    if (!agendamentoId || !agendamento) {
      return;
    }

    this.erroPagamento.set(null);
    this.confirmandoPagamento.set(true);

    const formaPagamento = this.formaPagamentoEscolhida();

    this.agendamentoService.confirmarPagamento(agendamentoId, formaPagamento).subscribe({
      next: () => {
        this.confirmandoPagamento.set(false);
        this.fecharPagamento();
        this.sucessoPagamentoInfo.set({
          quadraNome: agendamento.quadraNome,
          data: agendamento.data,
          horaInicio: agendamento.horaInicio,
          formaPagamento
        });
        this.carregar();
      },
      error: (err) => {
        this.confirmandoPagamento.set(false);
        this.erroPagamento.set(err?.error?.message ?? 'Não foi possível confirmar o pagamento.');
      }
    });
  }

  rotuloFormaPagamento(forma: FormaPagamento): string {
    if (forma === 'Cartao') {
      return 'cartão';
    }
    return forma === 'Pix' ? 'Pix' : 'dinheiro';
  }

  fecharSucessoPagamento(): void {
    this.sucessoPagamentoInfo.set(null);
  }
}
