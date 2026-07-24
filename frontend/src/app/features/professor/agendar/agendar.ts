import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
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
  readonly horariosSelecionados = signal<Set<string>>(new Set());
  readonly modalConfirmacaoAberto = signal(false);
  readonly mensagemConfirmacao = signal('');
  readonly quadraId = signal('');

  readonly dataMinima = hoje();
  data = hoje();

  readonly bloqueadaPorAprovacao = computed(() => this.auth.professorAprovado() === false);

  readonly quadraSelecionada = computed<Quadra | null>(
    () => this.quadras().find((q) => q.id === this.quadraId()) ?? null
  );

  readonly taxaCalculada = computed(() => {
    const quadra = this.quadraSelecionada();
    return quadra ? quadra.taxaPorHora * this.horariosSelecionados().size : 0;
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
          this.quadraId.set(quadras[0].id);
          this.buscarHorarios();
        }
      },
      error: () => this.erro.set('Não foi possível carregar as quadras.')
    });
  }

  selecionarQuadra(id: string): void {
    this.quadraId.set(id);
    this.buscarHorarios();
  }

  buscarHorarios(): void {
    const quadraId = this.quadraId();
    if (!quadraId || !this.data) {
      return;
    }

    this.erro.set(null);
    this.horariosSelecionados.set(new Set());
    this.carregandoSlots.set(true);

    this.quadraService.horariosDisponiveis(quadraId, this.data).subscribe({
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

  toggleSlot(slot: HorarioSlot): void {
    this.horariosSelecionados.update((atual) => {
      const novo = new Set(atual);
      if (novo.has(slot.horaInicio)) {
        novo.delete(slot.horaInicio);
      } else {
        novo.add(slot.horaInicio);
      }
      return novo;
    });
  }

  limparSelecao(): void {
    this.horariosSelecionados.set(new Set());
  }

  confirmarAgendamento(): void {
    const horarios = Array.from(this.horariosSelecionados()).sort();
    if (horarios.length === 0) {
      return;
    }

    this.erro.set(null);
    this.salvando.set(true);

    const quadraId = this.quadraId();
    const chamadas = horarios.map((horaInicio) =>
      this.agendamentoService
        .criar({ quadraId, data: this.data, horaInicio })
        .pipe(
          map(() => ({ horaInicio, sucesso: true as const })),
          catchError((err) =>
            of({
              horaInicio,
              sucesso: false as const,
              mensagem: err?.error?.message ?? 'Erro desconhecido'
            })
          )
        )
    );

    forkJoin(chamadas).subscribe((resultados) => {
      this.salvando.set(false);
      const sucessos = resultados.filter((r) => r.sucesso);
      const falhas = resultados.filter((r) => !r.sucesso);

      if (falhas.length > 0) {
        const horariosFalha = falhas.map((f) => f.horaInicio.slice(0, 5)).join(', ');
        this.erro.set(
          sucessos.length > 0
            ? `${sucessos.length} aula(s) agendada(s). Não foi possível reservar: ${horariosFalha}.`
            : `Não foi possível agendar: ${horariosFalha}.`
        );
      }

      this.limparSelecao();
      this.buscarHorarios();

      if (sucessos.length > 0) {
        this.mensagemConfirmacao.set(
          sucessos.length === 1
            ? `Aula agendada às ${sucessos[0].horaInicio.slice(0, 5)}!`
            : `${sucessos.length} aulas agendadas com sucesso!`
        );
        this.modalConfirmacaoAberto.set(true);
        setTimeout(() => this.irParaMeusAgendamentos(), ATRASO_REDIRECIONAMENTO_MS);
      }
    });
  }

  irParaMeusAgendamentos(): void {
    this.router.navigateByUrl('/professor/meus-agendamentos');
  }
}
