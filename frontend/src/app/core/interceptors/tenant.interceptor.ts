import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MASTER_SUBDOMAIN, TenantService } from '../services/tenant.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const subdominio = inject(TenantService).subdominio();

  // "arenapass" nunca é um espaço real — não faz sentido mandar como tenant.
  if (!subdominio || subdominio === MASTER_SUBDOMAIN) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { 'X-Tenant': subdominio }
    })
  );
};
