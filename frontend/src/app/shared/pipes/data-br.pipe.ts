import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dataBr' })
export class DataBrPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    if (!valor) {
      return '';
    }
    const [ano, mes, dia] = valor.split('-');
    if (!ano || !mes || !dia) {
      return valor;
    }
    return `${dia}/${mes}/${ano}`;
  }
}
