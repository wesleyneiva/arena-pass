import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-paginador',
  imports: [],
  templateUrl: './paginador.html'
})
export class Paginador {
  readonly paginaAtual = input.required<number>();
  readonly totalPaginas = input.required<number>();
  readonly mudarPagina = output<number>();

  anterior(): void {
    if (this.paginaAtual() > 1) {
      this.mudarPagina.emit(this.paginaAtual() - 1);
    }
  }

  proxima(): void {
    if (this.paginaAtual() < this.totalPaginas()) {
      this.mudarPagina.emit(this.paginaAtual() + 1);
    }
  }
}
