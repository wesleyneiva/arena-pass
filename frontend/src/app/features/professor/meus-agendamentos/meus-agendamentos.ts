import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { ConviteService } from '../../../core/services/convite.service';
import { Agendamento } from '../../../core/models/agendamento.models';
import { ConviteResumo } from '../../../core/models/convite.models';

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

  alunoNome = '';
  alunoCpf = '';

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
}
