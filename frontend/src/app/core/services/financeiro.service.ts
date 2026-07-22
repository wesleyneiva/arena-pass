import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FaturamentoMensal } from '../models/financeiro.models';

@Injectable({ providedIn: 'root' })
export class FinanceiroService {
  constructor(private readonly http: HttpClient) {}

  faturamentoMensal(ano: number, mes: number): Observable<FaturamentoMensal> {
    return this.http.get<FaturamentoMensal>(`${environment.apiUrl}/financeiro/faturamento-mensal`, {
      params: { ano, mes }
    });
  }
}
