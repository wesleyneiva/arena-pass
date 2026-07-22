import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FinanceiroService } from '../../../core/services/financeiro.service';
import { FaturamentoMensal } from '../../../core/models/financeiro.models';

@Component({
  selector: 'app-financeiro-admin',
  imports: [FormsModule],
  templateUrl: './financeiro-admin.html'
})
export class FinanceiroAdmin implements OnInit {
  readonly faturamento = signal<FaturamentoMensal | null>(null);
  readonly carregando = signal(false);

  ano = new Date().getFullYear();
  mes = new Date().getMonth() + 1;

  constructor(private readonly financeiroService: FinanceiroService) {}

  ngOnInit(): void {
    this.buscar();
  }

  buscar(): void {
    this.carregando.set(true);
    this.financeiroService.faturamentoMensal(this.ano, this.mes).subscribe({
      next: (faturamento) => {
        this.faturamento.set(faturamento);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
