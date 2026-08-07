import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtribuirAssinaturaRequest, Fatura, PainelFaturamento } from '../models/faturamento.models';

@Injectable({ providedIn: 'root' })
export class FaturamentoService {
  constructor(private readonly http: HttpClient) {}

  obterPainel(): Observable<PainelFaturamento> {
    return this.http.get<PainelFaturamento>(`${environment.apiUrl}/faturamento/painel`);
  }

  atribuirAssinatura(espacoId: string, request: AtribuirAssinaturaRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/espacos/${espacoId}/assinatura`, request);
  }

  listarFaturas(espacoId: string): Observable<Fatura[]> {
    return this.http.get<Fatura[]>(`${environment.apiUrl}/espacos/${espacoId}/faturas`);
  }

  marcarFaturaPaga(faturaId: string, dataPagamento?: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/faturas/${faturaId}/pagar`, { dataPagamento: dataPagamento ?? null });
  }
}
