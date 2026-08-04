import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalidadeService } from '../../../core/services/modalidade.service';
import { QuadraService } from '../../../core/services/quadra.service';
import { Modalidade } from '../../../core/models/modalidade.models';
import { Quadra } from '../../../core/models/quadra.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

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
  modalidadeNome = '';
  horaAbertura = '07:00';
  horaFechamento = '23:00';
  duracaoSlotMinutos = 60;
  taxaPorHora = 80;
  ativa = true;

  constructor(
    private readonly quadraService: QuadraService,
    private readonly modalidadeService: ModalidadeService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregarQuadras();
    this.carregarModalidades();
  }

  carregarModalidades(): void {
    this.modalidadeService.listar().subscribe({
      next: (modalidades) => this.modalidades.set(modalidades),
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
    this.modalidadeNome = quadra.modalidadeNome;
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
    this.modalidadeNome = '';
    this.taxaPorHora = 80;
    this.ativa = true;
  }

  async salvar(): Promise<void> {
    this.erro.set(null);

    const payload = {
      nome: this.nome,
      modalidadeNome: this.modalidadeNome,
      horaAbertura: this.horaAbertura,
      horaFechamento: this.horaFechamento,
      duracaoSlotMinutos: this.duracaoSlotMinutos,
      taxaPorHora: this.taxaPorHora
    };

    const editandoId = this.editandoId();
    if (editandoId) {
      const confirmado = await this.confirmDialog.confirmar({
        titulo: 'Salvar alterações',
        mensagem: `Salvar as alterações da quadra "${this.nome}"?`,
        textoConfirmar: 'Salvar',
        aoConfirmar: () => this.quadraService.atualizar(editandoId, { ...payload, ativa: this.ativa })
      });
      if (confirmado) {
        this.cancelarEdicao();
        this.carregarQuadras();
        this.carregarModalidades();
      }
      return;
    }

    this.salvando.set(true);
    this.quadraService.criar(payload).subscribe({
      next: () => {
        this.salvando.set(false);
        this.cancelarEdicao();
        this.carregarQuadras();
        this.carregarModalidades();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar a quadra.');
      }
    });
  }

  async excluir(quadra: Quadra): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir quadra',
      mensagem: `Excluir a quadra "${quadra.nome}"? Essa ação não pode ser desfeita.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo',
      aoConfirmar: () => this.quadraService.excluir(quadra.id)
    });
    if (confirmado) {
      this.carregarQuadras();
    }
  }
}
