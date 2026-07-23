import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarPerfilRequest,
  AuthResult,
  ConfirmarCodigoRegistroProfessorRequest,
  LoginRequest,
  SolicitarCodigoRegistroProfessorRequest
} from '../models/auth.models';

const STORAGE_KEY = 'arenapass.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authState = signal<AuthResult | null>(this.lerDoStorage());

  readonly usuario = computed(() => this.authState());
  readonly estaAutenticado = computed(() => this.authState() !== null);
  readonly role = computed(() => this.authState()?.role ?? null);
  readonly professorId = computed(() => this.authState()?.professorId ?? null);
  readonly professorAprovado = computed(() => this.authState()?.professorAprovado ?? null);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((resultado) => this.salvarSessao(resultado))
    );
  }

  solicitarCodigoRegistroProfessor(request: SolicitarCodigoRegistroProfessorRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/registrar-professor/solicitar-codigo`, request);
  }

  confirmarCodigoRegistroProfessor(
    request: ConfirmarCodigoRegistroProfessorRequest
  ): Observable<{ professorId: string }> {
    return this.http.post<{ professorId: string }>(
      `${environment.apiUrl}/auth/registrar-professor/confirmar-codigo`,
      request
    );
  }

  atualizarPerfil(request: AtualizarPerfilRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/auth/perfil`, request).pipe(
      tap(() => {
        const atual = this.authState();
        if (atual) {
          this.salvarSessao({ ...atual, email: request.email });
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.authState.set(null);
  }

  obterToken(): string | null {
    return this.authState()?.token ?? null;
  }

  private salvarSessao(resultado: AuthResult): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(resultado));
    this.authState.set(resultado);
  }

  private lerDoStorage(): AuthResult | null {
    const bruto = localStorage.getItem(STORAGE_KEY);
    if (!bruto) {
      return null;
    }

    try {
      return JSON.parse(bruto) as AuthResult;
    } catch {
      return null;
    }
  }
}
