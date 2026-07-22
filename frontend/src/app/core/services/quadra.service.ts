import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarQuadraRequest, CriarQuadraRequest, HorarioSlot, Quadra } from '../models/quadra.models';

@Injectable({ providedIn: 'root' })
export class QuadraService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Quadra[]> {
    return this.http.get<Quadra[]>(`${environment.apiUrl}/quadras`);
  }

  criar(request: CriarQuadraRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiUrl}/quadras`, request);
  }

  atualizar(id: string, request: AtualizarQuadraRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/quadras/${id}`, request);
  }

  horariosDisponiveis(quadraId: string, data: string): Observable<HorarioSlot[]> {
    return this.http.get<HorarioSlot[]>(
      `${environment.apiUrl}/quadras/${quadraId}/horarios-disponiveis`,
      { params: { data } }
    );
  }
}
