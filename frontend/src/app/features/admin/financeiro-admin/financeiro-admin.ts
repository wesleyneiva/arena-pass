import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FinanceiroService } from '../../../core/services/financeiro.service';
import { FaturamentoPeriodo } from '../../../core/models/financeiro.models';
import { FaturamentoChart } from '../../../shared/faturamento-chart/faturamento-chart';

type Preset = 'mes' | 'trimestre' | 'semestre' | 'ano';

function paraIso(data: Date): string {
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}

function primeiroDiaDoMes(ano: number, mes: number): Date {
  return new Date(ano, mes, 1);
}

function ultimoDiaDoMes(ano: number, mes: number): Date {
  return new Date(ano, mes + 1, 0);
}

@Component({
  selector: 'app-financeiro-admin',
  imports: [FormsModule, FaturamentoChart],
  templateUrl: './financeiro-admin.html'
})
export class FinanceiroAdmin implements OnInit {
  readonly faturamento = signal<FaturamentoPeriodo | null>(null);
  readonly carregando = signal(false);
  readonly presetAtivo = signal<Preset>('mes');

  readonly presets: { valor: Preset; rotulo: string }[] = [
    { valor: 'mes', rotulo: 'Este mês' },
    { valor: 'trimestre', rotulo: 'Últimos 3 meses' },
    { valor: 'semestre', rotulo: 'Últimos 6 meses' },
    { valor: 'ano', rotulo: 'Este ano' }
  ];

  dataInicio = '';
  dataFim = '';

  constructor(private readonly financeiroService: FinanceiroService) {}

  ngOnInit(): void {
    this.aplicarPreset('mes');
  }

  aplicarPreset(preset: Preset): void {
    this.presetAtivo.set(preset);
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = hoje.getMonth();

    switch (preset) {
      case 'mes':
        this.dataInicio = paraIso(primeiroDiaDoMes(ano, mes));
        this.dataFim = paraIso(ultimoDiaDoMes(ano, mes));
        break;
      case 'trimestre':
        this.dataInicio = paraIso(primeiroDiaDoMes(ano, mes - 2));
        this.dataFim = paraIso(ultimoDiaDoMes(ano, mes));
        break;
      case 'semestre':
        this.dataInicio = paraIso(primeiroDiaDoMes(ano, mes - 5));
        this.dataFim = paraIso(ultimoDiaDoMes(ano, mes));
        break;
      case 'ano':
        this.dataInicio = paraIso(primeiroDiaDoMes(ano, 0));
        this.dataFim = paraIso(ultimoDiaDoMes(ano, 11));
        break;
    }

    this.buscar();
  }

  buscar(): void {
    this.carregando.set(true);
    this.financeiroService.faturamento(this.dataInicio, this.dataFim).subscribe({
      next: (faturamento) => {
        this.faturamento.set(faturamento);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
