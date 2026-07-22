import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-professor-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './professor-layout.html'
})
export class ProfessorLayout {
  constructor(
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
