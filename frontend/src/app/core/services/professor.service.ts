import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Professor } from '../models/professor.models';

@Injectable({ providedIn: 'root' })
export class ProfessorService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Professor[]> {
    return this.http.get<Professor[]>(`${environment.apiUrl}/professores`);
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
