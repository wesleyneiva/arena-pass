import { Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';

export type ConfirmVariante = 'padrao' | 'perigo';

export interface ConfirmOpcoes {
  titulo: string;
  mensagem: string;
  textoConfirmar?: string;
  textoCancelar?: string;
  variante?: ConfirmVariante;
  aoConfirmar?: () => Observable<unknown>;
  somenteConfirmar?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  readonly aberto = signal(false);
  readonly titulo = signal('');
  readonly mensagem = signal('');
  readonly textoConfirmar = signal('Confirmar');
  readonly textoCancelar = signal('Cancelar');
  readonly variante = signal<ConfirmVariante>('padrao');
  readonly processando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly somenteConfirmar = signal(false);

  private resolver: ((valor: boolean) => void) | null = null;
  private acao: (() => Observable<unknown>) | null = null;

  confirmar(opcoes: ConfirmOpcoes): Promise<boolean> {
    this.titulo.set(opcoes.titulo);
    this.mensagem.set(opcoes.mensagem);
    this.textoConfirmar.set(opcoes.textoConfirmar ?? 'Confirmar');
    this.textoCancelar.set(opcoes.textoCancelar ?? 'Cancelar');
    this.variante.set(opcoes.variante ?? 'padrao');
    this.processando.set(false);
    this.erro.set(null);
    this.somenteConfirmar.set(opcoes.somenteConfirmar ?? false);
    this.acao = opcoes.aoConfirmar ?? null;
    this.aberto.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
    });
  }

  confirmarAcao(): void {
    if (!this.acao) {
      this.fechar(true);
      return;
    }

    this.erro.set(null);
    this.processando.set(true);

    this.acao().subscribe({
      next: () => {
        this.processando.set(false);
        this.fechar(true);
      },
      error: (err) => {
        this.processando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível concluir a ação.');
      }
    });
  }

  cancelarAcao(): void {
    if (this.processando()) {
      return;
    }
    this.fechar(false);
  }

  private fechar(valor: boolean): void {
    this.aberto.set(false);
    this.acao = null;
    this.resolver?.(valor);
    this.resolver = null;
  }
}
