import { Component, OnInit, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FaturamentoService } from '../../../core/services/faturamento.service';
import { EstatisticasAnuais, PainelFaturamento } from '../../../core/models/faturamento.models';
import { MiniBarChart } from '../../../shared/mini-bar-chart/mini-bar-chart';

type FiltroTile = 'pago' | 'atrasado' | 'ativo' | 'comPlano' | null;

@Component({
  selector: 'app-dashboard-master',
  imports: [DecimalPipe, MiniBarChart],
  templateUrl: './dashboard-master.html'
})
export class DashboardMaster implements OnInit {
  readonly painel = signal<PainelFaturamento | null>(null);
  readonly estatisticas = signal<EstatisticasAnuais | null>(null);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);
  readonly marcandoPagaId = signal<string | null>(null);

  readonly formatarMoeda = (valor: number) => 'R$ ' + valor.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly formatarInteiro = (valor: number) => valor.toLocaleString('pt-BR');

  readonly filtroAtivo = signal<FiltroTile>(null);

  readonly clientesFiltrados = computed(() => {
    const clientes = this.painel()?.clientes ?? [];
    const filtro = this.filtroAtivo();

    switch (filtro) {
      case 'pago': return clientes.filter((c) => c.status === 'Pago');
      case 'atrasado': return clientes.filter((c) => c.status === 'Atrasado');
      case 'ativo': return clientes.filter((c) => c.espacoAtivo);
      case 'comPlano': return clientes.filter((c) => c.status !== 'SemAssinatura');
      default: return clientes;
    }
  });

  constructor(private readonly faturamentoService: FaturamentoService) {}

  alternarFiltro(valor: FiltroTile): void {
    this.filtroAtivo.set(this.filtroAtivo() === valor ? null : valor);
  }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.faturamentoService.obterPainel().subscribe({
      next: (painel) => {
        this.painel.set(painel);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar o painel.');
      }
    });

    this.faturamentoService.obterEstatisticasAnuais().subscribe({
      next: (estatisticas) => this.estatisticas.set(estatisticas),
      error: () => {
        // Gráficos são um complemento — se falhar, o painel principal continua útil.
      }
    });
  }

  rotuloStatus(status: string): string {
    switch (status) {
      case 'Pago': return 'Em dia';
      case 'Pendente': return 'Pendente';
      case 'Atrasado': return 'Atrasado';
      default: return 'Sem plano';
    }
  }

  classeStatus(status: string): string {
    switch (status) {
      case 'Pago': return 'bg-emerald-50 text-emerald-700';
      case 'Pendente': return 'bg-blue-50 text-blue-700';
      case 'Atrasado': return 'bg-red-50 text-red-700';
      default: return 'bg-slate-100 text-slate-500';
    }
  }

  marcarPaga(faturaId: string): void {
    this.marcandoPagaId.set(faturaId);
    this.faturamentoService.marcarFaturaPaga(faturaId).subscribe({
      next: () => {
        this.marcandoPagaId.set(null);
        this.carregar();
      },
      error: (err) => {
        this.marcandoPagaId.set(null);
        this.erro.set(err?.error?.message ?? 'Não foi possível marcar a fatura como paga.');
      }
    });
  }
}
