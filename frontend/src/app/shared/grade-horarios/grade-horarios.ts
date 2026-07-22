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

  selecionar(slot: HorarioSlot): void {
    if (!slot.livre) {
      return;
    }
    this.slotSelecionado.emit(slot);
  }
}
