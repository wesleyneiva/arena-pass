import { Injectable, signal } from '@angular/core';

export type ConfirmVariante = 'padrao' | 'perigo';

export interface ConfirmOpcoes {
  titulo: string;
  mensagem: string;
  textoConfirmar?: string;
  textoCancelar?: string;
  variante?: ConfirmVariante;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  readonly aberto = signal(false);
  readonly titulo = signal('');
  readonly mensagem = signal('');
  readonly textoConfirmar = signal('Confirmar');
  readonly textoCancelar = signal('Cancelar');
  readonly variante = signal<ConfirmVariante>('padrao');

  private resolver: ((valor: boolean) => void) | null = null;

  confirmar(opcoes: ConfirmOpcoes): Promise<boolean> {
    this.titulo.set(opcoes.titulo);
    this.mensagem.set(opcoes.mensagem);
    this.textoConfirmar.set(opcoes.textoConfirmar ?? 'Confirmar');
    this.textoCancelar.set(opcoes.textoCancelar ?? 'Cancelar');
    this.variante.set(opcoes.variante ?? 'padrao');
    this.aberto.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
    });
  }

  confirmarAcao(): void {
    this.aberto.set(false);
    this.resolver?.(true);
    this.resolver = null;
  }

  cancelarAcao(): void {
    this.aberto.set(false);
    this.resolver?.(false);
    this.resolver = null;
  }
}
