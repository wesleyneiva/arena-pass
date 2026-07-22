import { Component, OnInit, signal } from '@angular/core';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/professor.models';

@Component({
  selector: 'app-professores-admin',
  imports: [],
  templateUrl: './professores-admin.html'
})
export class ProfessoresAdmin implements OnInit {
  readonly professores = signal<Professor[]>([]);

  constructor(private readonly professorService: ProfessorService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.professorService.listar().subscribe((professores) => this.professores.set(professores));
  }

  aprovar(id: string): void {
    this.professorService.aprovar(id).subscribe(() => this.carregar());
  }
}
