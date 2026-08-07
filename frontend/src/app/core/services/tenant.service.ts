import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly slug = signal<string | null>(this.resolver());

  readonly subdominio = this.slug.asReadonly();

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
