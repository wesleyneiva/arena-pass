import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalidadeService } from '../../../core/services/modalidade.service';
import { QuadraService } from '../../../core/services/quadra.service';
import { Modalidade } from '../../../core/models/modalidade.models';
import { Quadra } from '../../../core/models/quadra.models';

@Component({
  selector: 'app-quadras-admin',
  imports: [FormsModule],
  templateUrl: './quadras-admin.html'
})
export class QuadrasAdmin implements OnInit {
  readonly quadras = signal<Quadra[]>([]);
  readonly modalidades = signal<Modalidade[]>([]);
  readonly erro = signal<string | null>(null);
  readonly salvando = signal(false);

  nome = '';
  modalidadeId = '';
  horaAbertura = '07:00';
  horaFechamento = '23:00';
  duracaoSlotMinutos = 60;

  constructor(
    private readonly quadraService: QuadraService,
    private readonly modalidadeService: ModalidadeService
  ) {}

  ngOnInit(): void {
    this.carregarQuadras();
    this.modalidadeService.listar().subscribe((modalidades) => {
      this.modalidades.set(modalidades);
      if (modalidades.length > 0) {
        this.modalidadeId = modalidades[0].id;
      }
    });
  }

  carregarQuadras(): void {
    this.quadraService.listar().subscribe((quadras) => this.quadras.set(quadras));
  }

  criar(): void {
    this.erro.set(null);
    this.salvando.set(true);

    this.quadraService
      .criar({
        nome: this.nome,
        modalidadeId: this.modalidadeId,
        horaAbertura: this.horaAbertura,
        horaFechamento: this.horaFechamento,
        duracaoSlotMinutos: this.duracaoSlotMinutos
      })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.nome = '';
          this.carregarQuadras();
        },
        error: (err) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível criar a quadra.');
        }
      });
  }
}
