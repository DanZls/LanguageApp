import { Component, inject } from '@angular/core';

import { NotificationService } from '../../services/notification.service';
import { AppTheme, UserSettingsService } from '../../services/user-settings.service';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  templateUrl: './user-settings.component.html',
  styleUrl: './user-settings.component.css',
})
export class UserSettingsComponent {
  readonly userSettings = inject(UserSettingsService);
  private readonly notificationService = inject(NotificationService);

  setUserEmail(event: Event): void {
    const email = (event.target as HTMLInputElement).value;
    const previousEmail = this.userSettings.userEmail();

    this.userSettings.setUserEmail(email);

    if (this.userSettings.userEmail() === previousEmail && email.trim().length === 0) {
      this.notificationService.warning('Email cannot be empty.', 2200);
      return;
    }

    this.notificationService.success('Email updated.', 1800);
  }

  setAutoPlayAudio(event: Event): void {
    const autoPlayAudio = (event.target as HTMLInputElement).checked;

    this.userSettings.setAutoPlayAudio(autoPlayAudio);
    this.notificationService.info(autoPlayAudio ? 'Auto-play enabled.' : 'Auto-play disabled.', 1800);
  }

  setLanguage1Tag(event: Event): void {
    const language1Tag = (event.target as HTMLSelectElement).value;

    this.userSettings.setLanguage1Tag(language1Tag);
    this.notificationService.info('Primary language updated.', 1800);
  }

  setLanguage2Tag(event: Event): void {
    const language2Tag = (event.target as HTMLSelectElement).value;

    this.userSettings.setLanguage2Tag(language2Tag);
    this.notificationService.info('Learning language updated.', 1800);
  }

  setAudioLoudness(event: Event): void {
    const loudnessPercent = (event.target as HTMLInputElement).valueAsNumber;

    this.userSettings.setAudioLoudness(loudnessPercent / 100);
  }

  setTheme(theme: AppTheme): void {
    this.userSettings.setTheme(theme);
    this.notificationService.info(`${theme === 'light' ? 'Light' : 'Dark'} mode enabled.`, 1800);
  }
}
