import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Admin, AtualizarAdminRequest, CriarAdminRequest } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Admin[]> {
    return this.http.get<Admin[]>(`${environment.apiUrl}/admins`);
  }

  criar(request: CriarAdminRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/admins`, request);
  }

  atualizar(id: string, request: AtualizarAdminRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/admins/${id}`, request);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/admins/${id}`);
  }
}
