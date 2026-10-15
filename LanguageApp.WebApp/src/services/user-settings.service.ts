import { computed, effect, Injectable, signal, untracked } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { AppTheme, UserSettingsDto } from '../models/models';
import { TranslationHttpService } from './http.service';
import { LocalStorageService } from './local-storage.service';

export type { AppTheme } from '../models/models';

const saveDebounceMs = 400;

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  readonly languages = ['ru', 'de'];

  // The email stays in localStorage; everything else lives in the backend (UserSettings table)
  readonly userEmail = signal('dan.zloschastiev@gmail.com');
  readonly autoPlayAudio = signal(false);
  readonly audioLoudness = signal(0.8);
  readonly audioLoudnessPercent = computed(() => Math.round(this.audioLoudness() * 100));
  readonly language1Tag = signal(this.languages[0]);
  readonly language2Tag = signal(this.languages[1]);
  readonly theme = signal<AppTheme>('light');

  private saveTimer: ReturnType<typeof setTimeout> | undefined;
  // Bumped on every local change so a slow initial load never overwrites newer edits
  private localChangeVersion = 0;

  constructor(
    private readonly localStorageService: LocalStorageService,
    private readonly httpService: TranslationHttpService,
  ) {
    const storedUserEmail = this.localStorageService.getUserEmail();

    if (storedUserEmail) {
      this.userEmail.set(storedUserEmail);
    }

    effect(() => {
      document.documentElement.setAttribute('data-theme', this.theme());
    });

    effect(() => {
      const userEmail = this.userEmail();

      this.localStorageService.setUserEmail(userEmail);
      untracked(() => void this.loadSettings(userEmail));
    });
  }

  setUserEmail(userEmail: string): void {
    const normalizedEmail = userEmail.trim();

    if (normalizedEmail.length === 0) {
      return;
    }

    this.userEmail.set(normalizedEmail);
  }

  setAutoPlayAudio(autoPlayAudio: boolean): void {
    this.autoPlayAudio.set(autoPlayAudio);
    this.scheduleSave();
  }

  setAudioLoudness(audioLoudness: number): void {
    this.audioLoudness.set(this.normalizeAudioLoudness(audioLoudness));
    this.scheduleSave();
  }

  setLanguage1Tag(language1Tag: string): void {
    if (!this.languages.includes(language1Tag)) {
      return;
    }

    this.language1Tag.set(language1Tag);
    this.scheduleSave();
  }

  setLanguage2Tag(language2Tag: string): void {
    if (!this.languages.includes(language2Tag)) {
      return;
    }

    this.language2Tag.set(language2Tag);
    this.scheduleSave();
  }

  setTheme(theme: AppTheme): void {
    this.theme.set(theme);
    this.scheduleSave();
  }

  toggleTheme(): void {
    this.setTheme(this.theme() === 'light' ? 'dark' : 'light');
  }

  private async loadSettings(userEmail: string): Promise<void> {
    const versionAtRequest = this.localChangeVersion;

    try {
      const settings = await firstValueFrom(this.httpService.getUserSettings(userEmail));

      if (versionAtRequest === this.localChangeVersion && userEmail === this.userEmail()) {
        this.applySettings(settings);
      }
    } catch {
      // The error is already reported by the http service; keep the current values
    }
  }

  private applySettings(settings: UserSettingsDto): void {
    this.autoPlayAudio.set(settings.autoPlayAudio);
    this.audioLoudness.set(this.normalizeAudioLoudness(settings.audioLoudness));
    if (this.languages.includes(settings.language1Tag)) {
      this.language1Tag.set(settings.language1Tag);
    }
    if (this.languages.includes(settings.language2Tag)) {
      this.language2Tag.set(settings.language2Tag);
    }
    this.theme.set(settings.theme === 'dark' ? 'dark' : 'light');
  }

  private scheduleSave(): void {
    this.localChangeVersion++;
    clearTimeout(this.saveTimer);
    this.saveTimer = setTimeout(() => void this.saveSettings(), saveDebounceMs);
  }

  private async saveSettings(): Promise<void> {
    const settings: UserSettingsDto = {
      autoPlayAudio: this.autoPlayAudio(),
      audioLoudness: this.audioLoudness(),
      language1Tag: this.language1Tag(),
      language2Tag: this.language2Tag(),
      theme: this.theme(),
    };

    try {
      await firstValueFrom(this.httpService.saveUserSettings(this.userEmail(), settings));
    } catch {
      // The error is already reported by the http service
    }
  }

  private normalizeAudioLoudness(audioLoudness: number): number {
    if (!Number.isFinite(audioLoudness)) {
      return 0.8;
    }

    return Math.min(1, Math.max(0, audioLoudness));
  }
}
