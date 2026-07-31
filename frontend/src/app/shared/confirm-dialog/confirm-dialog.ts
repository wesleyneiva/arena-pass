import { Component, inject } from '@angular/core';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.html'
})
export class ConfirmDialog {
  protected readonly servico = inject(ConfirmDialogService);

  protected classeConfirmar(): string {
    const perigo = this.servico.variante() === 'perigo';
    if (this.servico.processando()) {
      return perigo
        ? 'animate-shimmer bg-[length:200%_100%] bg-gradient-to-r from-red-400 via-red-200 to-red-400'
        : 'animate-shimmer bg-[length:200%_100%] bg-gradient-to-r from-blue-400 via-blue-200 to-blue-400';
    }
    return perigo ? 'bg-red-600 hover:bg-red-700' : 'bg-blue-500 hover:bg-blue-600';
  }
}
