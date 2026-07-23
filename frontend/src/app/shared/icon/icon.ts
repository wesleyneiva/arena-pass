import { Component, input } from '@angular/core';

export type IconName =
  | 'grid'
  | 'users'
  | 'calendar'
  | 'chart'
  | 'logout'
  | 'menu'
  | 'close'
  | 'clipboard'
  | 'qrcode'
  | 'plus'
  | 'check'
  | 'ban'
  | 'refresh'
  | 'pencil'
  | 'shield';

@Component({
  selector: 'app-icon',
  imports: [],
  templateUrl: './icon.html'
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly size = input(20);
}
