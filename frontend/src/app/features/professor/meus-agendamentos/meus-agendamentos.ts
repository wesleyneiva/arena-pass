import { Component, OnInit, signal } from '@angular/core';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { Agendamento } from '../../../core/models/agendamento.models';

@Component({
  selector: 'app-meus-agendamentos',
  imports: [],
  templateUrl: './meus-agendamentos.html'
})
export class MeusAgendamentos implements OnInit {
  readonly agendamentos = signal<Agendamento[]>([]);

  constructor(private readonly agendamentoService: AgendamentoService) {}

  ngOnInit(): void {
    this.agendamentoService.meus().subscribe((agendamentos) => this.agendamentos.set(agendamentos));
  }
}
