import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html'
})
export class AdminLayout {
  constructor(
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
