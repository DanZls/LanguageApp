import { computed, effect, Injectable, signal } from '@angular/core';

import { LocalStorageService } from './local-storage.service';

export type AppTheme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  readonly languages = ['ru', 'de'];

  readonly userEmail = signal('dan.zloschastiev@gmail.com');
  readonly autoPlayAudio = signal(false);
  readonly audioLoudness = signal(0.8);
  readonly audioLoudnessPercent = computed(() => Math.round(this.audioLoudness() * 100));
  readonly language1Tag = signal(this.languages[0]);
  readonly language2Tag = signal(this.languages[1]);
  readonly theme = signal<AppTheme>('light');

  constructor(private readonly localStorageService: LocalStorageService) {
    const storedUserEmail = this.localStorageService.getUserEmail();
    const storedAutoPlayAudio = this.localStorageService.getAutoPlayAudio();
    const storedAudioLoudness = this.localStorageService.getAudioLoudness();
    const storedLanguage1Tag = this.localStorageService.getLanguage1Tag();
    const storedLanguage2Tag = this.localStorageService.getLanguage2Tag();
    const storedTheme = this.localStorageService.getTheme();

    if (storedUserEmail) {
      this.userEmail.set(storedUserEmail);
    }

    if (storedAutoPlayAudio === 'true' || storedAutoPlayAudio === 'false') {
      this.autoPlayAudio.set(storedAutoPlayAudio === 'true');
    }

    if (storedAudioLoudness !== null) {
      this.audioLoudness.set(this.normalizeAudioLoudness(Number(storedAudioLoudness)));
    }

    if (storedLanguage1Tag && this.languages.includes(storedLanguage1Tag)) {
      this.language1Tag.set(storedLanguage1Tag);
    }

    if (storedLanguage2Tag && this.languages.includes(storedLanguage2Tag)) {
      this.language2Tag.set(storedLanguage2Tag);
    }

    if (storedTheme === 'light' || storedTheme === 'dark') {
      this.theme.set(storedTheme);
    }

    effect(() => {
      const currentTheme = this.theme();

      document.documentElement.setAttribute('data-theme', currentTheme);
      this.localStorageService.setTheme(currentTheme);
      this.localStorageService.setUserEmail(this.userEmail());
      this.localStorageService.setAutoPlayAudio(String(this.autoPlayAudio()));
      this.localStorageService.setAudioLoudness(String(this.audioLoudness()));
      this.localStorageService.setLanguage1Tag(this.language1Tag());
      this.localStorageService.setLanguage2Tag(this.language2Tag());
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
  }

  setAudioLoudness(audioLoudness: number): void {
    this.audioLoudness.set(this.normalizeAudioLoudness(audioLoudness));
  }

  setLanguage1Tag(language1Tag: string): void {
    if (!this.languages.includes(language1Tag)) {
      return;
    }

    this.language1Tag.set(language1Tag);
  }

  setLanguage2Tag(language2Tag: string): void {
    if (!this.languages.includes(language2Tag)) {
      return;
    }

    this.language2Tag.set(language2Tag);
  }

  setTheme(theme: AppTheme): void {
    this.theme.set(theme);
  }

  toggleTheme(): void {
    this.theme.update((theme) => (theme === 'light' ? 'dark' : 'light'));
  }

  private normalizeAudioLoudness(audioLoudness: number): number {
    if (!Number.isFinite(audioLoudness)) {
      return 0.8;
    }

    return Math.min(1, Math.max(0, audioLoudness));
  }
}
