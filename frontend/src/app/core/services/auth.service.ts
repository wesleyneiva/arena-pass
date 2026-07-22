import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, RegistrarProfessorRequest } from '../models/auth.models';

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

  registrarProfessor(request: RegistrarProfessorRequest): Observable<{ professorId: string }> {
    return this.http.post<{ professorId: string }>(
      `${environment.apiUrl}/auth/registrar-professor`,
      request
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
