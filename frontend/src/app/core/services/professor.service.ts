import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarProfessorRequest,
  CriarProfessorRequest,
  Professor,
  VerificarEmailProfessorResult
} from '../models/professor.models';

@Injectable({ providedIn: 'root' })
export class ProfessorService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Professor[]> {
    return this.http.get<Professor[]>(`${environment.apiUrl}/professores`);
  }

  verificarEmail(email: string): Observable<VerificarEmailProfessorResult> {
    return this.http.get<VerificarEmailProfessorResult>(`${environment.apiUrl}/professores/verificar-email`, {
      params: { email }
    });
  }

  criar(request: CriarProfessorRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/professores`, request);
  }

  atualizar(id: string, request: AtualizarProfessorRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/professores/${id}`, request);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/professores/${id}`);
  }

  aprovar(id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/professores/${id}/aprovar`, {});
  }

  suspender(id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/professores/${id}/suspender`, {});
  }

  reativar(id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/professores/${id}/reativar`, {});
  }
}
