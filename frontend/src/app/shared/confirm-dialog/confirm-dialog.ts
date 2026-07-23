import { Component, inject } from '@angular/core';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.html'
})
export class ConfirmDialog {
  protected readonly servico = inject(ConfirmDialogService);
}
