import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QuadraService } from '../../../core/services/quadra.service';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { Quadra, HorarioSlot } from '../../../core/models/quadra.models';
import { GradeHorarios } from '../../../shared/grade-horarios/grade-horarios';

function hoje(): string {
  const agora = new Date();
  const mes = String(agora.getMonth() + 1).padStart(2, '0');
  const dia = String(agora.getDate()).padStart(2, '0');
  return `${agora.getFullYear()}-${mes}-${dia}`;
}

@Component({
  selector: 'app-agendar',
  imports: [FormsModule, GradeHorarios],
  templateUrl: './agendar.html'
})
export class Agendar implements OnInit {
  readonly quadras = signal<Quadra[]>([]);
  readonly slots = signal<HorarioSlot[]>([]);
  readonly erro = signal<string | null>(null);
  readonly sucesso = signal<string | null>(null);
  readonly carregandoSlots = signal(false);
  readonly salvando = signal(false);

  quadraId = '';
  data = hoje();
  taxaValor = 80;

  constructor(
    private readonly quadraService: QuadraService,
    private readonly agendamentoService: AgendamentoService
  ) {}

  ngOnInit(): void {
    this.quadraService.listar().subscribe((quadras) => {
      this.quadras.set(quadras);
      if (quadras.length > 0) {
        this.quadraId = quadras[0].id;
        this.buscarHorarios();
      }
    });
  }

  buscarHorarios(): void {
    if (!this.quadraId || !this.data) {
      return;
    }

    this.erro.set(null);
    this.sucesso.set(null);
    this.carregandoSlots.set(true);

    this.quadraService.horariosDisponiveis(this.quadraId, this.data).subscribe({
      next: (slots) => {
        this.slots.set(slots);
        this.carregandoSlots.set(false);
      },
      error: () => {
        this.carregandoSlots.set(false);
        this.erro.set('Não foi possível carregar os horários dessa quadra.');
      }
    });
  }

  agendar(slot: HorarioSlot): void {
    this.erro.set(null);
    this.sucesso.set(null);
    this.salvando.set(true);

    this.agendamentoService
      .criar({ quadraId: this.quadraId, data: this.data, horaInicio: slot.horaInicio, taxaValor: this.taxaValor })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.sucesso.set(`Aula agendada às ${slot.horaInicio.slice(0, 5)}!`);
          this.buscarHorarios();
        },
        error: (err) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível agendar esse horário.');
          this.buscarHorarios();
        }
      });
  }
}
