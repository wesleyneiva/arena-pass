import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { QuadraService } from '../../../core/services/quadra.service';
import { AgendamentoService } from '../../../core/services/agendamento.service';
import { AuthService } from '../../../core/services/auth.service';
import { Quadra, HorarioSlot } from '../../../core/models/quadra.models';
import { GradeHorarios } from '../../../shared/grade-horarios/grade-horarios';

function hoje(): string {
  const agora = new Date();
  const mes = String(agora.getMonth() + 1).padStart(2, '0');
  const dia = String(agora.getDate()).padStart(2, '0');
  return `${agora.getFullYear()}-${mes}-${dia}`;
}

const ATRASO_REDIRECIONAMENTO_MS = 1800;

@Component({
  selector: 'app-agendar',
  imports: [FormsModule, GradeHorarios],
  templateUrl: './agendar.html'
})
export class Agendar implements OnInit {
  readonly quadras = signal<Quadra[]>([]);
  readonly slots = signal<HorarioSlot[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregandoSlots = signal(false);
  readonly salvando = signal(false);
  readonly horaInicioSelecionada = signal<string | null>(null);
  readonly quantidadeHoras = signal(1);
  readonly modalConfirmacaoAberto = signal(false);
  readonly mensagemConfirmacao = signal('');

  quadraId = '';
  data = hoje();

  readonly bloqueadaPorAprovacao = computed(() => this.auth.professorAprovado() === false);

  readonly quadraSelecionada = computed<Quadra | null>(
    () => this.quadras().find((q) => q.id === this.quadraId) ?? null
  );

  readonly taxaCalculada = computed(() => {
    const quadra = this.quadraSelecionada();
    return quadra ? quadra.taxaPorHora * this.quantidadeHoras() : 0;
  });

  readonly podeSelecionarDuasHoras = computed(() => {
    const horaInicio = this.horaInicioSelecionada();
    if (!horaInicio) {
      return false;
    }

    const slots = this.slots();
    const indice = slots.findIndex((s) => s.horaInicio === horaInicio);
    return indice !== -1 && indice + 1 < slots.length && slots[indice + 1].livre;
  });

  constructor(
    private readonly quadraService: QuadraService,
    private readonly agendamentoService: AgendamentoService,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.quadraService.listar().subscribe({
      next: (quadras) => {
        this.quadras.set(quadras);
        if (quadras.length > 0) {
          this.quadraId = quadras[0].id;
          this.buscarHorarios();
        }
      },
      error: () => this.erro.set('Não foi possível carregar as quadras.')
    });
  }

  buscarHorarios(): void {
    if (!this.quadraId || !this.data) {
      return;
    }

    this.erro.set(null);
    this.horaInicioSelecionada.set(null);
    this.carregandoSlots.set(true);

    this.quadraService.horariosDisponiveis(this.quadraId, this.data).subscribe({
      next: (slots) => {
        this.slots.set(slots);
        this.carregandoSlots.set(false);
      },
      error: () => {
        this.carregandoSlots.set(false);
        this.erro.set('Não foi possível carregar os horários dessa quadra.');
      }
    });
  }

  selecionarSlot(slot: HorarioSlot): void {
    this.horaInicioSelecionada.set(slot.horaInicio);
    this.quantidadeHoras.set(1);
  }

  escolherDuracao(horas: number): void {
    if (horas === 2 && !this.podeSelecionarDuasHoras()) {
      return;
    }
    this.quantidadeHoras.set(horas);
  }

  cancelarSelecao(): void {
    this.horaInicioSelecionada.set(null);
    this.quantidadeHoras.set(1);
  }

  confirmarAgendamento(): void {
    const horaInicio = this.horaInicioSelecionada();
    if (!horaInicio) {
      return;
    }

    this.erro.set(null);
    this.salvando.set(true);

    this.agendamentoService
      .criar({
        quadraId: this.quadraId,
        data: this.data,
        horaInicio,
        quantidadeHoras: this.quantidadeHoras()
      })
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.mensagemConfirmacao.set(`Aula agendada às ${horaInicio.slice(0, 5)}!`);
          this.modalConfirmacaoAberto.set(true);
          this.cancelarSelecao();
          setTimeout(() => this.irParaMeusAgendamentos(), ATRASO_REDIRECIONAMENTO_MS);
        },
        error: (err) => {
          this.salvando.set(false);
          this.erro.set(err?.error?.message ?? 'Não foi possível agendar esse horário.');
          this.buscarHorarios();
        }
      });
  }

  irParaMeusAgendamentos(): void {
    this.router.navigateByUrl('/professor/meus-agendamentos');
  }
}
