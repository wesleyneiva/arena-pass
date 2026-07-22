import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Modalidade } from '../models/modalidade.models';

@Injectable({ providedIn: 'root' })
export class ModalidadeService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Modalidade[]> {
    return this.http.get<Modalidade[]>(`${environment.apiUrl}/modalidades`);
  }
}
