import { Component, input, output } from '@angular/core';
import { HorarioSlot } from '../../core/models/quadra.models';

@Component({
  selector: 'app-grade-horarios',
  imports: [],
  templateUrl: './grade-horarios.html'
})
export class GradeHorarios {
  readonly slots = input.required<HorarioSlot[]>();
  readonly slotSelecionado = output<HorarioSlot>();

  selecionar(slot: HorarioSlot): void {
    if (!slot.livre) {
      return;
    }
    this.slotSelecionado.emit(slot);
  }
}
