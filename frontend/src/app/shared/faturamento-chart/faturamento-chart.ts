import { Component, computed, input, signal } from '@angular/core';
import { FaturamentoMes } from '../../core/models/financeiro.models';

const MESES_ABREVIADOS = [
  'jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'
];

interface BarraChart {
  x: number;
  y: number;
  largura: number;
  altura: number;
  path: string;
  slotX: number;
  slotLargura: number;
  rotuloMes: string;
  rotuloCentroX: number;
  valor: number;
}

function pathBarraArredondada(x: number, y: number, largura: number, altura: number): string {
  const r = Math.max(0, Math.min(4, altura / 2, largura / 2));
  return [
    `M ${x} ${y + altura}`,
    `L ${x} ${y + r}`,
    `A ${r} ${r} 0 0 1 ${x + r} ${y}`,
    `L ${x + largura - r} ${y}`,
    `A ${r} ${r} 0 0 1 ${x + largura} ${y + r}`,
    `L ${x + largura} ${y + altura}`,
    'Z'
  ].join(' ');
}

interface LinhaGrade {
  y: number;
  rotulo: string;
}

const LARGURA_VIRTUAL = 640;
const ALTURA_VIRTUAL = 220;
const MARGEM_ESQUERDA = 52;
const ALTURA_EIXO_X = 24;
const TOPO_RESERVADO = 12;
const LARGURA_MAXIMA_BARRA = 24;

@Component({
  selector: 'app-faturamento-chart',
  imports: [],
  templateUrl: './faturamento-chart.html'
})
export class FaturamentoChart {
  readonly dados = input.required<FaturamentoMes[]>();
  readonly indiceHover = signal<number | null>(null);

  readonly larguraVirtual = LARGURA_VIRTUAL;
  readonly alturaVirtual = ALTURA_VIRTUAL;
  readonly margemEsquerda = MARGEM_ESQUERDA;
  readonly areaUtilAltura = ALTURA_VIRTUAL - ALTURA_EIXO_X;
  private readonly areaUtilLargura = LARGURA_VIRTUAL - MARGEM_ESQUERDA;

  private readonly valorMaximo = computed(() => {
    const maior = Math.max(0, ...this.dados().map((d) => d.total));
    return maior === 0 ? 1 : maior;
  });

  private valorParaY(valor: number): number {
    const max = this.valorMaximo();
    return this.areaUtilAltura - (valor / max) * (this.areaUtilAltura - TOPO_RESERVADO);
  }

  readonly linhasGrade = computed<LinhaGrade[]>(() => {
    const max = this.valorMaximo();
    return [0, max / 2, max].map((valor) => ({
      y: this.valorParaY(valor),
      rotulo: this.formatarMoedaCompacta(valor)
    }));
  });

  readonly barras = computed<BarraChart[]>(() => {
    const dados = this.dados();
    if (dados.length === 0) {
      return [];
    }

    const slotLargura = this.areaUtilLargura / dados.length;
    const largura = Math.min(LARGURA_MAXIMA_BARRA, slotLargura - 8);

    return dados.map((d, i) => {
      const y = this.valorParaY(d.total);
      const x = MARGEM_ESQUERDA + slotLargura * i + (slotLargura - largura) / 2;
      const altura = Math.max(1, this.areaUtilAltura - y);
      return {
        x,
        y,
        largura,
        altura,
        path: pathBarraArredondada(x, y, largura, altura),
        slotX: MARGEM_ESQUERDA + slotLargura * i,
        slotLargura,
        rotuloMes: `${MESES_ABREVIADOS[d.mes - 1]}/${String(d.ano).slice(-2)}`,
        rotuloCentroX: x + largura / 2,
        valor: d.total
      };
    });
  });

  formatarMoeda(valor: number): string {
    return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  private formatarMoedaCompacta(valor: number): string {
    if (valor >= 1000) {
      return `${(valor / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}mil`;
    }
    return `${Math.round(valor)}`;
  }
}
