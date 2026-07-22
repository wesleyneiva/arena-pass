import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalidadeService } from '../../../core/services/modalidade.service';
import { QuadraService } from '../../../core/services/quadra.service';
import { Modalidade } from '../../../core/models/modalidade.models';
import { Quadra } from '../../../core/models/quadra.models';
import { Icon } from '../../../shared/icon/icon';

@Component({
  selector: 'app-quadras-admin',
  imports: [FormsModule, Icon],
  templateUrl: './quadras-admin.html'
})
export class QuadrasAdmin implements OnInit {
  readonly quadras = signal<Quadra[]>([]);
  readonly modalidades = signal<Modalidade[]>([]);
  readonly erro = signal<string | null>(null);
  readonly erroLista = signal<string | null>(null);
  readonly salvando = signal(false);
  readonly carregando = signal(true);
  readonly editandoId = signal<string | null>(null);

  nome = '';
  modalidadeId = '';
  horaAbertura = '07:00';
  horaFechamento = '23:00';
  duracaoSlotMinutos = 60;
  taxaPorHora = 80;
  ativa = true;

  constructor(
    private readonly quadraService: QuadraService,
    private readonly modalidadeService: ModalidadeService
  ) {}

  ngOnInit(): void {
    this.carregarQuadras();
    this.modalidadeService.listar().subscribe({
      next: (modalidades) => {
        this.modalidades.set(modalidades);
        if (modalidades.length > 0) {
          this.modalidadeId = modalidades[0].id;
        }
      },
      error: () => this.erroLista.set('Não foi possível carregar as modalidades.')
    });
  }

  carregarQuadras(): void {
    this.carregando.set(true);
    this.erroLista.set(null);
    this.quadraService.listar().subscribe({
      next: (quadras) => {
        this.quadras.set(quadras);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erroLista.set(err?.error?.message ?? 'Não foi possível carregar as quadras.');
      }
    });
  }

  editar(quadra: Quadra): void {
    this.editandoId.set(quadra.id);
    this.nome = quadra.nome;
    this.modalidadeId = quadra.modalidadeId;
    this.horaAbertura = quadra.horaAbertura.slice(0, 5);
    this.horaFechamento = quadra.horaFechamento.slice(0, 5);
    this.duracaoSlotMinutos = quadra.duracaoSlotMinutos;
    this.taxaPorHora = quadra.taxaPorHora;
    this.ativa = quadra.ativa;
    this.erro.set(null);
  }

  cancelarEdicao(): void {
    this.editandoId.set(null);
    this.nome = '';
    this.taxaPorHora = 80;
    this.ativa = true;
  }

  salvar(): void {
    this.erro.set(null);
    this.salvando.set(true);

    const payload = {
      nome: this.nome,
      modalidadeId: this.modalidadeId,
      horaAbertura: this.horaAbertura,
      horaFechamento: this.horaFechamento,
      duracaoSlotMinutos: this.duracaoSlotMinutos,
      taxaPorHora: this.taxaPorHora
    };

    const aoConcluir = {
      next: () => {
        this.salvando.set(false);
        this.cancelarEdicao();
        this.carregarQuadras();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar a quadra.');
      }
    };

    const editandoId = this.editandoId();
    if (editandoId) {
      this.quadraService.atualizar(editandoId, { ...payload, ativa: this.ativa }).subscribe(aoConcluir);
    } else {
      this.quadraService.criar(payload).subscribe(aoConcluir);
    }
  }
}
