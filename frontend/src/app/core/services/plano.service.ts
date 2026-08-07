import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarPlanoRequest, CriarPlanoRequest, Plano } from '../models/plano.models';

@Injectable({ providedIn: 'root' })
export class PlanoService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Plano[]> {
    return this.http.get<Plano[]>(`${environment.apiUrl}/planos`);
  }

  criar(request: CriarPlanoRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/planos`, request);
  }

  atualizar(id: string, request: AtualizarPlanoRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/planos/${id}`, request);
  }

  atualizarStatus(id: string, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/planos/${id}/status`, { ativo });
  }
}
