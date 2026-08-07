import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarEspacoRequest,
  CriarEspacoRequest,
  Espaco,
  ResolverEspacoResult
} from '../models/espaco.models';

@Injectable({ providedIn: 'root' })
export class EspacoService {
  constructor(private readonly http: HttpClient) {}

  resolverAtual(): Observable<ResolverEspacoResult> {
    return this.http.get<ResolverEspacoResult>(`${environment.apiUrl}/espacos/resolver`);
  }

  listar(): Observable<Espaco[]> {
    return this.http.get<Espaco[]>(`${environment.apiUrl}/espacos`);
  }

  criar(request: CriarEspacoRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/espacos`, request);
  }

  atualizar(id: string, request: AtualizarEspacoRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/espacos/${id}`, request);
  }

  atualizarStatus(id: string, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/espacos/${id}/status`, { ativo });
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/espacos/${id}`);
  }
}
