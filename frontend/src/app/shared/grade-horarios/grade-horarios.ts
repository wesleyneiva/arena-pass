import { Component, input, output } from '@angular/core';
import { HorarioSlot } from '../../core/models/quadra.models';

@Component({
  selector: 'app-grade-horarios',
  imports: [],
  templateUrl: './grade-horarios.html'
})
export class GradeHorarios {
  readonly slots = input.required<HorarioSlot[]>();
  readonly horariosSelecionados = input<ReadonlySet<string>>(new Set());
  readonly bloqueada = input(false);
  readonly slotSelecionado = output<HorarioSlot>();

  estaSelecionado(slot: HorarioSlot): boolean {
    return this.horariosSelecionados().has(slot.horaInicio);
  }

  rotulo(slot: HorarioSlot): string {
    if (this.bloqueada()) {
      return 'Aguardando aprovação';
    }
    if (slot.livre) {
      return 'Livre';
    }
    return slot.agendamentoId ? 'Ocupado' : 'Indisponível';
  }

  intervalo(slot: HorarioSlot): string {
    const [h, m] = slot.horaFim.split(':').map(Number);
    const totalMinutos = ((h * 60 + m - 1) + 1440) % 1440;
    const hh = String(Math.floor(totalMinutos / 60)).padStart(2, '0');
    const mm = String(totalMinutos % 60).padStart(2, '0');
    return `${slot.horaInicio.slice(0, 5)} - ${hh}:${mm}`;
  }

  classesSlot(slot: HorarioSlot): Record<string, boolean> {
    if (this.bloqueada()) {
      return {
        'bg-slate-50': true,
        'border-slate-200': true,
        'text-slate-300': true,
        'cursor-not-allowed': true
      };
    }

    if (!slot.livre) {
      return {
        'bg-slate-50': true,
        'border-slate-200': true,
        'text-slate-400': true,
        'cursor-not-allowed': true
      };
    }

    if (this.estaSelecionado(slot)) {
      return {
        'bg-blue-500': true,
        'border-blue-500': true,
        'text-white': true,
        'shadow-md': true,
        'scale-[1.03]': true
      };
    }

    return {
      'bg-blue-50': true,
      'border-blue-200': true,
      'text-blue-600': true,
      'hover:border-blue-400': true,
      'hover:shadow-sm': true,
      'cursor-pointer': true
    };
  }

  selecionar(slot: HorarioSlot): void {
    if (!slot.livre || this.bloqueada()) {
      return;
    }
    this.slotSelecionado.emit(slot);
  }
}
