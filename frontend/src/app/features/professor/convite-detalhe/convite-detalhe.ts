import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConviteService } from '../../../core/services/convite.service';
import { ConviteDetalhes } from '../../../core/models/convite.models';
import { DataBrPipe } from '../../../shared/pipes/data-br.pipe';

@Component({
  selector: 'app-convite-detalhe',
  imports: [RouterLink, DataBrPipe],
  templateUrl: './convite-detalhe.html'
})
export class ConviteDetalhe implements OnInit {
  readonly convite = signal<ConviteDetalhes | null>(null);
  readonly erro = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly conviteService: ConviteService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.conviteService.obter(id).subscribe({
      next: (convite) => this.convite.set(convite),
      error: (err) => this.erro.set(err?.error?.message ?? 'Não foi possível carregar o convite.')
    });
  }

  imprimir(): void {
    window.print();
  }
}
