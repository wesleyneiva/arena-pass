import { Component, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PlanoService } from '../../../core/services/plano.service';
import { Plano } from '../../../core/models/plano.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-planos-master',
  imports: [FormsModule, Icon, DecimalPipe],
  templateUrl: './planos-master.html'
})
export class PlanosMaster implements OnInit {
  readonly planos = signal<Plano[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);

  readonly formularioAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly salvando = signal(false);

  novoNome = '';
  novoValorMensal: number | null = null;

  constructor(
    private readonly planoService: PlanoService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.planoService.listar().subscribe({
      next: (planos) => {
        this.planos.set(planos);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os planos.');
      }
    });
  }

  abrirNovo(): void {
    this.editandoId.set(null);
    this.novoNome = '';
    this.novoValorMensal = null;
    this.erro.set(null);
    this.formularioAberto.set(true);
  }

  editar(plano: Plano): void {
    this.editandoId.set(plano.id);
    this.novoNome = plano.nome;
    this.novoValorMensal = plano.valorMensal;
    this.erro.set(null);
    this.formularioAberto.set(true);
  }

  fecharFormulario(): void {
    this.formularioAberto.set(false);
    this.editandoId.set(null);
  }

  salvar(): void {
    if (this.novoValorMensal === null) {
      return;
    }

    this.erro.set(null);
    this.salvando.set(true);

    const aoConcluir = {
      next: () => {
        this.salvando.set(false);
        this.fecharFormulario();
        this.carregar();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar o plano.');
      }
    };

    const editandoId = this.editandoId();
    const dados = { nome: this.novoNome, valorMensal: this.novoValorMensal };

    if (editandoId) {
      this.planoService.atualizar(editandoId, dados).subscribe(aoConcluir);
    } else {
      this.planoService.criar(dados).subscribe(aoConcluir);
    }
  }

  async alternarStatus(plano: Plano): Promise<void> {
    const vaiDesativar = plano.ativo;
    const confirmado = await this.confirmDialog.confirmar({
      titulo: vaiDesativar ? 'Desativar plano' : 'Reativar plano',
      mensagem: vaiDesativar
        ? `Desativar "${plano.nome}"? Não poderá mais ser atribuído a novos espaços, mas quem já assina continua normalmente.`
        : `Reativar "${plano.nome}"?`,
      textoConfirmar: vaiDesativar ? 'Desativar' : 'Reativar',
      variante: vaiDesativar ? 'perigo' : 'padrao',
      aoConfirmar: () => this.planoService.atualizarStatus(plano.id, !vaiDesativar)
    });

    if (confirmado) {
      this.carregar();
    }
  }
}
