import { Component, ElementRef, OnInit, ViewChild, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FinanceiroService } from '../../../core/services/financeiro.service';
import { FaturamentoPeriodo } from '../../../core/models/financeiro.models';
import { FaturamentoChart } from '../../../shared/faturamento-chart/faturamento-chart';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/professor.models';

type Preset = 'mes' | 'trimestre' | 'semestre' | 'ano';

const MESES_COMPLETOS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
];

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

function formatarDataBr(iso: string): string {
  const [ano, mes, dia] = iso.split('-');
  return ano && mes && dia ? `${dia}/${mes}/${ano}` : iso;
}

function formatarMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function svgParaPngDataUrl(svg: SVGSVGElement, escala = 2): Promise<string> {
  return new Promise((resolve, reject) => {
    const viewBox = svg.viewBox.baseVal;
    const largura = viewBox.width || svg.clientWidth;
    const altura = viewBox.height || svg.clientHeight;

    const clone = svg.cloneNode(true) as SVGSVGElement;
    clone.setAttribute('width', String(largura));
    clone.setAttribute('height', String(altura));
    clone.setAttribute('xmlns', 'http://www.w3.org/2000/svg');

    const svgTexto = new XMLSerializer().serializeToString(clone);
    const dataUrlSvg = `data:image/svg+xml;base64,${btoa(unescape(encodeURIComponent(svgTexto)))}`;

    const imagem = new Image();
    imagem.onload = () => {
      const canvas = document.createElement('canvas');
      canvas.width = largura * escala;
      canvas.height = altura * escala;
      const ctx = canvas.getContext('2d');
      if (!ctx) {
        reject(new Error('Não foi possível criar o contexto do canvas.'));
        return;
      }
      ctx.fillStyle = '#ffffff';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(imagem, 0, 0, canvas.width, canvas.height);
      resolve(canvas.toDataURL('image/png'));
    };
    imagem.onerror = () => reject(new Error('Não foi possível renderizar o gráfico.'));
    imagem.src = dataUrlSvg;
  });
}

@Component({
  selector: 'app-financeiro-admin',
  imports: [FormsModule, FaturamentoChart],
  templateUrl: './financeiro-admin.html'
})
export class FinanceiroAdmin implements OnInit {
  @ViewChild('graficoWrapper') graficoWrapper?: ElementRef<HTMLElement>;

  readonly faturamento = signal<FaturamentoPeriodo | null>(null);
  readonly carregando = signal(false);
  readonly exportando = signal(false);
  readonly erroExportacao = signal<string | null>(null);
  readonly presetAtivo = signal<Preset>('mes');
  readonly professores = signal<Professor[]>([]);

  professorId = '';

  readonly presets: { valor: Preset; rotulo: string }[] = [
    { valor: 'mes', rotulo: 'Este mês' },
    { valor: 'trimestre', rotulo: 'Últimos 3 meses' },
    { valor: 'semestre', rotulo: 'Últimos 6 meses' },
    { valor: 'ano', rotulo: 'Este ano' }
  ];

  dataInicio = '';
  dataFim = '';

  constructor(
    private readonly financeiroService: FinanceiroService,
    private readonly professorService: ProfessorService
  ) {}

  ngOnInit(): void {
    this.professorService.listar().subscribe((professores) => this.professores.set(professores));
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

  nomeProfessorSelecionado(): string {
    return this.professores().find((p) => p.id === this.professorId)?.nome ?? '';
  }

  buscar(): void {
    this.carregando.set(true);
    this.financeiroService.faturamento(this.dataInicio, this.dataFim, this.professorId || undefined).subscribe({
      next: (faturamento) => {
        this.faturamento.set(faturamento);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }

  async exportarRelatorio(): Promise<void> {
    const f = this.faturamento();
    if (!f) {
      return;
    }

    this.erroExportacao.set(null);
    this.exportando.set(true);

    try {
      const [{ default: JsPdf }, { default: autoTable }] = await Promise.all([
        import('jspdf'),
        import('jspdf-autotable')
      ]);

      const doc = new JsPdf({ unit: 'pt' });
      const margem = 40;
      const larguraUtil = doc.internal.pageSize.getWidth() - margem * 2;
      let y = margem;

      doc.setFontSize(16);
      doc.setTextColor(20);
      doc.text('Relatório financeiro - ArenaPass', margem, y);
      y += 22;

      doc.setFontSize(10);
      doc.setTextColor(100);
      doc.text(`Período: ${formatarDataBr(f.dataInicio)} a ${formatarDataBr(f.dataFim)}`, margem, y);
      y += 14;
      doc.text(`Professor: ${this.professorId ? this.nomeProfessorSelecionado() : 'Todos os professores'}`, margem, y);
      y += 14;
      doc.text(`Gerado em: ${new Date().toLocaleString('pt-BR')}`, margem, y);
      y += 24;

      doc.setFontSize(13);
      doc.setTextColor(20);
      doc.text(`Total geral: ${formatarMoeda(f.totalGeral)}`, margem, y);
      y += 16;

      const svg = this.graficoWrapper?.nativeElement.querySelector('svg');
      if (svg) {
        const png = await svgParaPngDataUrl(svg);
        const alturaImagem = larguraUtil * (220 / 640);
        doc.addImage(png, 'PNG', margem, y, larguraUtil, alturaImagem);
        y += alturaImagem + 20;
      }

      autoTable(doc, {
        startY: y,
        margin: { left: margem, right: margem },
        head: [['Mês', 'Faturamento']],
        body: f.porMes.map((m) => [`${MESES_COMPLETOS[m.mes - 1]}/${m.ano}`, formatarMoeda(m.total)]),
        headStyles: { fillColor: [30, 41, 59] },
        styles: { fontSize: 9 }
      });

      const finalYMeses = (doc as unknown as { lastAutoTable: { finalY: number } }).lastAutoTable.finalY;

      if (f.porProfessor.length > 0) {
        autoTable(doc, {
          startY: finalYMeses + 20,
          margin: { left: margem, right: margem },
          head: [['Professor', 'Aulas', 'Valor total']],
          body: f.porProfessor.map((p) => [p.professorNome, String(p.totalAulas), formatarMoeda(p.valorTotal)]),
          headStyles: { fillColor: [30, 41, 59] },
          styles: { fontSize: 9 }
        });
      }

      doc.save(`relatorio-financeiro-${f.dataInicio}-a-${f.dataFim}.pdf`);
    } catch {
      this.erroExportacao.set('Não foi possível gerar o relatório.');
    } finally {
      this.exportando.set(false);
    }
  }
}
