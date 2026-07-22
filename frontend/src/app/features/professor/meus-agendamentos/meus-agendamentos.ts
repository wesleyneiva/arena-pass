import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { ConviteService } from '../../../core/services/convite.service';
import { Agendamento, FormaPagamento, PagamentoPix } from '../../../core/models/agendamento.models';
import { ConviteResumo } from '../../../core/models/convite.models';

type FiltroStatus = 'Todos' | 'PendentePagamento' | 'Confirmado' | 'Realizado' | 'Cancelado';

@Component({
  selector: 'app-meus-agendamentos',
  imports: [FormsModule, RouterLink],
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
      const bateStatus = status === 'Todos' || a.status === status;
      const bateQuadra = quadra === 'Todas' || a.quadraNome === quadra;
      const bateDataDe = dataDe === '' || a.data >= dataDe;
      const bateDataAte = dataAte === '' || a.data <= dataAte;
      const bateBusca = busca === '' || a.quadraNome.toLowerCase().includes(busca);
      return bateStatus && bateQuadra && bateDataDe && bateDataAte && bateBusca;
    });
  });

  readonly agendamentoEmPagamento = computed(() =>
    this.agendamentos().find((a) => a.id === this.modalPagamentoId()) ?? null
  );

  constructor(
    private readonly agendamentoService: AgendamentoService,
    private readonly conviteService: ConviteService
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

  cancelar(agendamentoId: string): void {
    if (!confirm('Cancelar esse agendamento?')) {
      return;
    }

    this.agendamentoService.cancelar(agendamentoId).subscribe({
      next: () => this.carregar(),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível cancelar.')
    });
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
    if (!agendamentoId) {
      return;
    }

    this.erroPagamento.set(null);
    this.confirmandoPagamento.set(true);

    this.agendamentoService.confirmarPagamento(agendamentoId, this.formaPagamentoEscolhida()).subscribe({
      next: () => {
        this.confirmandoPagamento.set(false);
        this.fecharPagamento();
        this.carregar();
      },
      error: (err) => {
        this.confirmandoPagamento.set(false);
        this.erroPagamento.set(err?.error?.message ?? 'Não foi possível confirmar o pagamento.');
      }
    });
  }
}
