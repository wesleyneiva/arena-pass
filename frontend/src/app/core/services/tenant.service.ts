import { Injectable, computed, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

// Subdomínio reservado exclusivamente para o Master (dono do SaaS) — nunca é um
// espaço/cliente real. Professor e AdminClube nunca logam aqui.
export const MASTER_SUBDOMAIN = 'arenapass';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly slug = signal<string | null>(this.resolver());

  readonly subdominio = this.slug.asReadonly();
  readonly ehDominioMaster = computed(() => this.slug() === MASTER_SUBDOMAIN);

  private resolver(): string | null {
    const doQuery = new URLSearchParams(window.location.search).get('tenant');
    if (doQuery) {
      return doQuery.trim().toLowerCase();
    }

    const { baseDomain } = environment;
    const hostname = window.location.hostname;

    if (baseDomain && hostname.endsWith(`.${baseDomain}`)) {
      const subdominio = hostname.slice(0, -(`.${baseDomain}`.length));
      if (subdominio && !subdominio.includes('.')) {
        return subdominio.toLowerCase();
      }
    }

    return environment.tenantPadrao?.toLowerCase() ?? null;
  }
}
