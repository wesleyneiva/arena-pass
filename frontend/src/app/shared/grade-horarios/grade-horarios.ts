import { Component, computed, input, output } from '@angular/core';
import { HorarioSlot } from '../../core/models/quadra.models';

@Component({
  selector: 'app-grade-horarios',
  imports: [],
  templateUrl: './grade-horarios.html'
})
export class GradeHorarios {
  readonly slots = input.required<HorarioSlot[]>();
  readonly horaInicioSelecionada = input<string | null>(null);
  readonly quantidadeHorasSelecionada = input(1);
  readonly bloqueada = input(false);
  readonly slotSelecionado = output<HorarioSlot>();

  readonly indicesCobertos = computed<Set<number>>(() => {
    const horaInicio = this.horaInicioSelecionada();
    if (!horaInicio) {
      return new Set();
    }

    const indiceInicio = this.slots().findIndex((s) => s.horaInicio === horaInicio);
    if (indiceInicio === -1) {
      return new Set();
    }

    const indices = new Set<number>();
    for (let i = 0; i < this.quantidadeHorasSelecionada(); i++) {
      indices.add(indiceInicio + i);
    }
    return indices;
  });

  estaSelecionado(index: number): boolean {
    return this.indicesCobertos().has(index);
  }

  rotulo(slot: HorarioSlot): string {
    if (this.bloqueada()) {
      return 'Aguardando aprovação';
    }
    return slot.livre ? 'Livre' : 'Ocupado';
  }

  classesSlot(slot: HorarioSlot, index: number): Record<string, boolean> {
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

    if (this.estaSelecionado(index)) {
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
