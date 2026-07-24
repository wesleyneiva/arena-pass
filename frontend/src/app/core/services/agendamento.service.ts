import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Agendamento, CriarAgendamentoRequest, FormaPagamento, PagamentoPix } from '../models/agendamento.models';

@Injectable({ providedIn: 'root' })
export class AgendamentoService {
  constructor(private readonly http: HttpClient) {}

  criar(request: CriarAgendamentoRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/agendamentos`, request);
  }

  meus(): Observable<Agendamento[]> {
    return this.http.get<Agendamento[]>(`${environment.apiUrl}/agendamentos/meus`);
  }

  listarTodos(): Observable<Agendamento[]> {
    return this.http.get<Agendamento[]>(`${environment.apiUrl}/agendamentos`);
  }

  confirmarPagamento(id: string, formaPagamento: FormaPagamento): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/agendamentos/${id}/confirmar-pagamento`, {
      formaPagamento
    });
  }

  obterPagamentoPix(id: string): Observable<PagamentoPix> {
    return this.http.get<PagamentoPix>(`${environment.apiUrl}/agendamentos/${id}/pagamento-pix`);
  }

  cancelar(id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/agendamentos/${id}/cancelar`, {});
  }
}
