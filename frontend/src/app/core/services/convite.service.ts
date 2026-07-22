import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConviteDetalhes, ConviteResumo, EmitirConviteRequest } from '../models/convite.models';

@Injectable({ providedIn: 'root' })
export class ConviteService {
  constructor(private readonly http: HttpClient) {}

  emitir(agendamentoId: string, request: EmitirConviteRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(
      `${environment.apiUrl}/agendamentos/${agendamentoId}/convites`,
      request
    );
  }

  listarPorAgendamento(agendamentoId: string): Observable<ConviteResumo[]> {
    return this.http.get<ConviteResumo[]>(`${environment.apiUrl}/agendamentos/${agendamentoId}/convites`);
  }

  obter(id: string): Observable<ConviteDetalhes> {
    return this.http.get<ConviteDetalhes>(`${environment.apiUrl}/convites/${id}`);
  }
}
