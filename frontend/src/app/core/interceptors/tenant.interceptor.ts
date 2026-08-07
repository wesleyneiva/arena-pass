import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantService } from '../services/tenant.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const subdominio = inject(TenantService).subdominio();

  if (!subdominio) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { 'X-Tenant': subdominio }
    })
  );
};
