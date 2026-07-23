import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

type Role = 'AdminClube' | 'Professor' | 'Master';

export function roleGuard(rolesPermitidos: Role | Role[]): CanActivateFn {
  const roles = Array.isArray(rolesPermitidos) ? rolesPermitidos : [rolesPermitidos];

  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.estaAutenticado()) {
      return router.parseUrl('/login');
    }

    if (!roles.includes(auth.role() as Role)) {
      return router.parseUrl('/login');
    }

    return true;
  };
}
