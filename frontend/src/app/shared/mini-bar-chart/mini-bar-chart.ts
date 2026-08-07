import { Component, computed, input } from '@angular/core';

interface Barra {
  mes: string;
  valor: number;
  x: number;
  largura: number;
  altura: number;
  y: number;
  maior: boolean;
}

const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

@Component({
  selector: 'app-mini-bar-chart',
  imports: [],
  templateUrl: './mini-bar-chart.html'
})
export class MiniBarChart {
  readonly valores = input.required<number[]>();
  readonly cor = input('#2a78d6');
  readonly formatarValor = input<(valor: number) => string>((valor) => valor.toLocaleString('pt-BR'));

  readonly larguraSvg = 400;
  readonly alturaSvg = 150;
  readonly linhaBase = 130;
  private readonly alturaPlot = 100;
  private readonly raio = 4;

  readonly maiorValor = computed(() => Math.max(...this.valores(), 1));

  readonly barras = computed<Barra[]>(() => {
    const valores = this.valores();
    const max = this.maiorValor();
    const n = valores.length || 1;
    const gap = 3;
    const largura = (this.larguraSvg - gap * (n + 1)) / n;

    return valores.map((valor, i) => {
      const altura = max > 0 ? (valor / max) * this.alturaPlot : 0;
      return {
        mes: MESES[i] ?? '',
        valor,
        x: gap + i * (largura + gap),
        largura,
        altura,
        y: this.linhaBase - altura,
        maior: valor === max && valor > 0
      };
    });
  });

  caminhoBarra(barra: Barra): string {
    const { x, y, largura, altura } = barra;
    if (altura <= 0) {
      return '';
    }
    const r = Math.min(this.raio, largura / 2, altura);
    const baseY = y + altura;
    return `M${x},${baseY} L${x},${y + r} Q${x},${y} ${x + r},${y} L${x + largura - r},${y} Q${x + largura},${y} ${x + largura},${y + r} L${x + largura},${baseY} Z`;
  }

  formatar(valor: number): string {
    return this.formatarValor()(valor);
  }
}
