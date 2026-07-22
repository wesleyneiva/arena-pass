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
    return slot.livre ? 'Livre' : 'Ocupado';
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
        'bg-blue-600': true,
        'border-blue-600': true,
        'text-white': true,
        'shadow-md': true,
        'scale-[1.03]': true
      };
    }

    return {
      'bg-blue-50': true,
      'border-blue-200': true,
      'text-blue-700': true,
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
