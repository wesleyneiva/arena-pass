import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export function roleGuard(roleEsperado: 'AdminClube' | 'Professor'): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.estaAutenticado()) {
      return router.parseUrl('/login');
    }

    if (auth.role() !== roleEsperado) {
      return router.parseUrl('/login');
    }

    return true;
  };
}
