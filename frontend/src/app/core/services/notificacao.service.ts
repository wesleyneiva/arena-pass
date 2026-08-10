import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Notificacao {
  id: string;
  titulo: string;
  mensagem: string;
  agendamentoId: string | null;
  lida: boolean;
  criadaEm: string;
}

export interface PainelNotificacoes {
  naoLidas: number;
  itens: Notificacao[];
}

const INTERVALO_POLLING_MS = 60_000;

@Injectable({ providedIn: 'root' })
export class NotificacaoService {
  readonly naoLidas = signal(0);

  private pollingId: ReturnType<typeof setInterval> | null = null;

  constructor(private readonly http: HttpClient) {}

  listar(limite = 20): Observable<PainelNotificacoes> {
    return this.http
      .get<PainelNotificacoes>(`${environment.apiUrl}/notificacoes`, { params: { limite } })
      .pipe(tap((painel) => this.naoLidas.set(painel.naoLidas)));
  }

  marcarTodasLidas(): Observable<void> {
    return this.http
      .post<void>(`${environment.apiUrl}/notificacoes/marcar-lidas`, {})
      .pipe(tap(() => this.naoLidas.set(0)));
  }

  iniciarPolling(): void {
    if (this.pollingId !== null) {
      return;
    }
    this.atualizarContagem();
    this.pollingId = setInterval(() => this.atualizarContagem(), INTERVALO_POLLING_MS);
  }

  pararPolling(): void {
    if (this.pollingId !== null) {
      clearInterval(this.pollingId);
      this.pollingId = null;
    }
  }

  private atualizarContagem(): void {
    // Erros são ignorados de propósito: o badge só fica desatualizado até o próximo tick.
    this.listar(1).subscribe({ error: () => {} });
  }
}
