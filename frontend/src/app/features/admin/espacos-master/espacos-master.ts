import { Component, OnInit, computed, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { EspacoService } from '../../../core/services/espaco.service';
import { AdminService } from '../../../core/services/admin.service';
import { PlanoService } from '../../../core/services/plano.service';
import { FaturamentoService } from '../../../core/services/faturamento.service';
import { Espaco } from '../../../core/models/espaco.models';
import { Plano } from '../../../core/models/plano.models';
import { EspacoFaturamento, Fatura } from '../../../core/models/faturamento.models';
import { Icon } from '../../../shared/icon/icon';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-espacos-master',
  imports: [FormsModule, Icon, DatePipe, DecimalPipe],
  templateUrl: './espacos-master.html'
})
export class EspacosMaster implements OnInit {
  readonly espacos = signal<Espaco[]>([]);
  readonly faturamentoPorEspaco = signal<Map<string, EspacoFaturamento>>(new Map());
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);

  readonly busca = signal('');
  readonly filtroStatus = signal<'todos' | 'ativo' | 'bloqueado'>('todos');

  readonly espacosFiltrados = computed(() => {
    const termo = this.busca().trim().toLowerCase();
    const status = this.filtroStatus();

    return this.espacos().filter((espaco) => {
      const bateBusca = !termo
        || espaco.nome.toLowerCase().includes(termo)
        || espaco.subdominio.toLowerCase().includes(termo);
      const bateStatus = status === 'todos'
        || (status === 'ativo' && espaco.ativo)
        || (status === 'bloqueado' && !espaco.ativo);
      return bateBusca && bateStatus;
    });
  });

  atualizarBusca(valor: string): void {
    this.busca.set(valor);
  }

  atualizarFiltroStatus(valor: string): void {
    this.filtroStatus.set(valor as 'todos' | 'ativo' | 'bloqueado');
  }

  readonly formularioEspacoAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly salvandoEspaco = signal(false);
  novoNomeEspaco = '';
  novoSubdominio = '';

  readonly espacoParaAdmin = signal<Espaco | null>(null);
  readonly salvandoAdmin = signal(false);
  readonly erroAdmin = signal<string | null>(null);
  novoNomeAdmin = '';
  novoEmailAdmin = '';
  novaSenhaAdmin = '';

  readonly espacoParaCobranca = signal<Espaco | null>(null);
  readonly carregandoCobranca = signal(false);
  readonly salvandoAssinatura = signal(false);
  readonly marcandoPaga = signal(false);
  readonly erroCobranca = signal<string | null>(null);
  readonly planosAtivos = signal<Plano[]>([]);
  readonly faturasDoEspaco = signal<Fatura[]>([]);
  planoIdSelecionado = '';
  diaVencimentoSelecionado: number | null = null;

  constructor(
    private readonly espacoService: EspacoService,
    private readonly adminService: AdminService,
    private readonly planoService: PlanoService,
    private readonly faturamentoService: FaturamentoService,
    private readonly confirmDialog: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    forkJoin({
      espacos: this.espacoService.listar(),
      painel: this.faturamentoService.obterPainel()
    }).subscribe({
      next: ({ espacos, painel }) => {
        this.espacos.set(espacos);
        this.faturamentoPorEspaco.set(new Map(painel.clientes.map((c) => [c.espacoId, c])));
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os espaços.');
      }
    });
  }

  rotuloCobranca(espacoId: string): string {
    const status = this.faturamentoPorEspaco().get(espacoId)?.status;
    switch (status) {
      case 'Pago': return 'Em dia';
      case 'Pendente': return 'Pendente';
      case 'Atrasado': return 'Atrasado';
      default: return 'Sem plano';
    }
  }

  classeCobranca(espacoId: string): string {
    const status = this.faturamentoPorEspaco().get(espacoId)?.status;
    switch (status) {
      case 'Pago': return 'bg-emerald-50 text-emerald-700';
      case 'Pendente': return 'bg-blue-50 text-blue-700';
      case 'Atrasado': return 'bg-red-50 text-red-700';
      default: return 'bg-slate-100 text-slate-500';
    }
  }

  abrirCobranca(espaco: Espaco): void {
    this.espacoParaCobranca.set(espaco);
    this.erroCobranca.set(null);
    this.carregandoCobranca.set(true);
    this.faturasDoEspaco.set([]);

    const faturamentoAtual = this.faturamentoPorEspaco().get(espaco.id);
    this.planoIdSelecionado = '';
    this.diaVencimentoSelecionado = faturamentoAtual?.diaVencimento ?? null;

    forkJoin({
      planos: this.planoService.listar(),
      faturas: this.faturamentoService.listarFaturas(espaco.id)
    }).subscribe({
      next: ({ planos, faturas }) => {
        this.planosAtivos.set(planos.filter((p) => p.ativo));
        this.faturasDoEspaco.set(faturas);
        const planoAtual = planos.find((p) => p.nome === faturamentoAtual?.planoNome);
        this.planoIdSelecionado = planoAtual?.id ?? this.planosAtivos()[0]?.id ?? '';
        this.carregandoCobranca.set(false);
      },
      error: (err) => {
        this.carregandoCobranca.set(false);
        this.erroCobranca.set(err?.error?.message ?? 'Não foi possível carregar a cobrança.');
      }
    });
  }

  fecharCobranca(): void {
    this.espacoParaCobranca.set(null);
  }

  salvarAssinatura(): void {
    const espaco = this.espacoParaCobranca();
    if (!espaco || !this.planoIdSelecionado || !this.diaVencimentoSelecionado) {
      return;
    }

    this.erroCobranca.set(null);
    this.salvandoAssinatura.set(true);
    this.faturamentoService
      .atribuirAssinatura(espaco.id, {
        planoId: this.planoIdSelecionado,
        diaVencimento: this.diaVencimentoSelecionado
      })
      .subscribe({
        next: () => {
          this.salvandoAssinatura.set(false);
          this.carregar();
          this.abrirCobranca(espaco);
        },
        error: (err) => {
          this.salvandoAssinatura.set(false);
          this.erroCobranca.set(err?.error?.message ?? 'Não foi possível salvar a assinatura.');
        }
      });
  }

  marcarFaturaAtualPaga(): void {
    const espaco = this.espacoParaCobranca();
    const faturaId = this.faturamentoPorEspaco().get(espaco?.id ?? '')?.faturaAtualId;
    if (!espaco || !faturaId) {
      return;
    }

    this.marcandoPaga.set(true);
    this.faturamentoService.marcarFaturaPaga(faturaId).subscribe({
      next: () => {
        this.marcandoPaga.set(false);
        this.carregar();
        this.abrirCobranca(espaco);
      },
      error: (err) => {
        this.marcandoPaga.set(false);
        this.erroCobranca.set(err?.error?.message ?? 'Não foi possível marcar a fatura como paga.');
      }
    });
  }

  abrirNovoEspaco(): void {
    this.editandoId.set(null);
    this.novoNomeEspaco = '';
    this.novoSubdominio = '';
    this.erro.set(null);
    this.formularioEspacoAberto.set(true);
  }

  editarEspaco(espaco: Espaco): void {
    this.editandoId.set(espaco.id);
    this.novoNomeEspaco = espaco.nome;
    this.novoSubdominio = espaco.subdominio;
    this.erro.set(null);
    this.formularioEspacoAberto.set(true);
  }

  fecharNovoEspaco(): void {
    this.formularioEspacoAberto.set(false);
    this.editandoId.set(null);
  }

  private semDiacriticos(valor: string): string {
    return valor.normalize('NFD').replace(new RegExp('[\\u0300-\\u036f]', 'g'), '');
  }

  // Aceita o que o usuário digitar (inclusive colando o domínio inteiro, tipo
  // "personaltennis.wnlabs.com.br" ou "https://personaltennis...") e reduz pra só a
  // parte do subdomínio — sem forçar hífen entre palavras, só remove o que não pode
  // fazer parte de um subdomínio.
  private limparSubdominio(valor: string): string {
    return this.semDiacriticos(valor.trim().toLowerCase())
      .replace(/^https?:\/\//, '')
      .split('.')[0]
      .replace(/[^a-z0-9-]/g, '')
      .replace(/(^-+|-+$)/g, '');
  }

  limparSubdominioDigitado(): void {
    this.novoSubdominio = this.limparSubdominio(this.novoSubdominio);
  }

  sugerirSubdominio(): void {
    if (this.novoSubdominio || this.editandoId()) {
      return;
    }
    // Sugestão a partir do nome não insere hífen entre palavras — só junta tudo
    // (ex: "Personal Tennis" -> "personaltennis"). O usuário pode editar livremente.
    this.novoSubdominio = this.semDiacriticos(this.novoNomeEspaco.toLowerCase()).replace(/[^a-z0-9]/g, '');
  }

  salvarEspaco(): void {
    this.erro.set(null);
    this.salvandoEspaco.set(true);

    const aoConcluir = {
      next: () => {
        this.salvandoEspaco.set(false);
        this.fecharNovoEspaco();
        this.carregar();
      },
      error: (err: { error?: { message?: string } }) => {
        this.salvandoEspaco.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível salvar o espaço.');
      }
    };

    const editandoId = this.editandoId();
    const dados = { nome: this.novoNomeEspaco, subdominio: this.limparSubdominio(this.novoSubdominio) };

    if (editandoId) {
      this.espacoService.atualizar(editandoId, dados).subscribe(aoConcluir);
    } else {
      this.espacoService.criar(dados).subscribe(aoConcluir);
    }
  }

  async alternarStatus(espaco: Espaco): Promise<void> {
    const vaiBloquear = espaco.ativo;
    const confirmado = await this.confirmDialog.confirmar({
      titulo: vaiBloquear ? 'Bloquear espaço' : 'Reativar espaço',
      mensagem: vaiBloquear
        ? `Bloquear "${espaco.nome}"? O acesso de admin e professores desse espaço é cortado na hora — use em caso de inadimplência.`
        : `Reativar "${espaco.nome}"? Admin e professores voltam a acessar normalmente.`,
      textoConfirmar: vaiBloquear ? 'Bloquear' : 'Reativar',
      variante: vaiBloquear ? 'perigo' : 'padrao',
      aoConfirmar: () => this.espacoService.atualizarStatus(espaco.id, !vaiBloquear)
    });

    if (confirmado) {
      this.carregar();
    }
  }

  async excluirEspaco(espaco: Espaco): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Excluir espaço',
      mensagem: `Excluir "${espaco.nome}"? Só é possível se ele ainda não tiver admin, quadra ou professor vinculado.`,
      textoConfirmar: 'Excluir',
      variante: 'perigo',
      aoConfirmar: () => this.espacoService.excluir(espaco.id)
    });

    if (confirmado) {
      this.carregar();
    }
  }

  abrirCriarAdmin(espaco: Espaco): void {
    this.novoNomeAdmin = '';
    this.novoEmailAdmin = '';
    this.novaSenhaAdmin = '';
    this.erroAdmin.set(null);
    this.espacoParaAdmin.set(espaco);
  }

  fecharCriarAdmin(): void {
    this.espacoParaAdmin.set(null);
  }

  salvarAdmin(): void {
    const espaco = this.espacoParaAdmin();
    if (!espaco) {
      return;
    }

    this.erroAdmin.set(null);
    this.salvandoAdmin.set(true);
    this.adminService
      .criar({
        nome: this.novoNomeAdmin,
        email: this.novoEmailAdmin,
        senha: this.novaSenhaAdmin,
        espacoId: espaco.id
      })
      .subscribe({
        next: () => {
          this.salvandoAdmin.set(false);
          this.fecharCriarAdmin();
        },
        error: (err) => {
          this.salvandoAdmin.set(false);
          this.erroAdmin.set(err?.error?.message ?? 'Não foi possível criar o administrador.');
        }
      });
  }
}
