import { Component, OnInit, signal } from '@angular/core';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { Agendamento } from '../../../core/models/agendamento.models';

@Component({
  selector: 'app-agendamentos-admin',
  imports: [],
  templateUrl: './agendamentos-admin.html'
})
export class AgendamentosAdmin implements OnInit {
  readonly agendamentos = signal<Agendamento[]>([]);

  constructor(private readonly agendamentoService: AgendamentoService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.agendamentoService.listarTodos().subscribe((agendamentos) => this.agendamentos.set(agendamentos));
  }

  confirmarPagamento(id: string): void {
    this.agendamentoService.confirmarPagamento(id).subscribe(() => this.carregar());
  }
}
