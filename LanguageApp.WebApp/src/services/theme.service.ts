import { effect, Injectable } from '@angular/core';

import { UserSettingsService } from './user-settings.service';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme: UserSettingsService['theme'];

  constructor(private readonly userSettingsService: UserSettingsService) {
    this.theme = this.userSettingsService.theme;

    effect(() => {
      document.documentElement.setAttribute('data-theme', this.theme());
    });
  }

  toggle(): void {
    this.userSettingsService.toggleTheme();
  }
}
