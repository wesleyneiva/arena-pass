import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FaturamentoPeriodo } from '../models/financeiro.models';

@Injectable({ providedIn: 'root' })
export class FinanceiroService {
  constructor(private readonly http: HttpClient) {}

  faturamento(dataInicio: string, dataFim: string, professorId?: string): Observable<FaturamentoPeriodo> {
    const params: Record<string, string> = { dataInicio, dataFim };
    if (professorId) {
      params['professorId'] = professorId;
    }
    return this.http.get<FaturamentoPeriodo>(`${environment.apiUrl}/financeiro/faturamento`, { params });
  }
}
