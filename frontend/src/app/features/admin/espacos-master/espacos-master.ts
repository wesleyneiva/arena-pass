import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EspacoService } from '../../../core/services/espaco.service';
import { AdminService } from '../../../core/services/admin.service';
import { Espaco } from '../../../core/models/espaco.models';
import { Icon } from '../../../shared/icon/icon';

@Component({
  selector: 'app-espacos-master',
  imports: [FormsModule, Icon],
  templateUrl: './espacos-master.html'
})
export class EspacosMaster implements OnInit {
  readonly espacos = signal<Espaco[]>([]);
  readonly erro = signal<string | null>(null);
  readonly carregando = signal(true);

  readonly formularioEspacoAberto = signal(false);
  readonly salvandoEspaco = signal(false);
  novoNomeEspaco = '';
  novoSubdominio = '';

  readonly espacoParaAdmin = signal<Espaco | null>(null);
  readonly salvandoAdmin = signal(false);
  readonly erroAdmin = signal<string | null>(null);
  novoNomeAdmin = '';
  novoEmailAdmin = '';
  novaSenhaAdmin = '';

  constructor(
    private readonly espacoService: EspacoService,
    private readonly adminService: AdminService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.espacoService.listar().subscribe({
      next: (espacos) => {
        this.espacos.set(espacos);
        this.carregando.set(false);
      },
      error: (err) => {
        this.carregando.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível carregar os espaços.');
      }
    });
  }

  abrirNovoEspaco(): void {
    this.novoNomeEspaco = '';
    this.novoSubdominio = '';
    this.erro.set(null);
    this.formularioEspacoAberto.set(true);
  }

  fecharNovoEspaco(): void {
    this.formularioEspacoAberto.set(false);
  }

  sugerirSubdominio(): void {
    if (this.novoSubdominio) {
      return;
    }
    this.novoSubdominio = this.novoNomeEspaco
      .toLowerCase()
      .normalize('NFD')
      .replace(new RegExp('[\\u0300-\\u036f]', 'g'), '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  }

  salvarEspaco(): void {
    this.erro.set(null);
    this.salvandoEspaco.set(true);
    this.espacoService.criar({ nome: this.novoNomeEspaco, subdominio: this.novoSubdominio }).subscribe({
      next: () => {
        this.salvandoEspaco.set(false);
        this.fecharNovoEspaco();
        this.carregar();
      },
      error: (err) => {
        this.salvandoEspaco.set(false);
        this.erro.set(err?.error?.message ?? 'Não foi possível criar o espaço.');
      }
    });
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
