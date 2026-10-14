import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LocalStorageService {
  private readonly userEmailKey = 'language-app.user-email';
  private readonly themeKey = 'language-app.theme';
  private readonly autoPlayAudioKey = 'language-app.auto-play-audio';
  private readonly audioLoudnessKey = 'language-app.audio-loudness';
  private readonly language1TagKey = 'language-app.language1-tag';
  private readonly language2TagKey = 'language-app.language2-tag';

  getUserEmail(): string | null {
    return this.getValue(this.userEmailKey);
  }

  setUserEmail(userEmail: string): void {
    this.setValue(this.userEmailKey, userEmail);
  }


  getTheme(): string | null {
    return this.getValue(this.themeKey);
  }

  setTheme(theme: string): void {
    this.setValue(this.themeKey, theme);
  }

  getAutoPlayAudio(): string | null {
    return this.getValue(this.autoPlayAudioKey);
  }

  setAutoPlayAudio(autoPlayAudio: string): void {
    this.setValue(this.autoPlayAudioKey, autoPlayAudio);
  }

  getAudioLoudness(): string | null {
    return this.getValue(this.audioLoudnessKey);
  }

  setAudioLoudness(audioLoudness: string): void {
    this.setValue(this.audioLoudnessKey, audioLoudness);
  }

  getLanguage1Tag(): string | null {
    return this.getValue(this.language1TagKey);
  }

  setLanguage1Tag(language1Tag: string): void {
    this.setValue(this.language1TagKey, language1Tag);
  }

  getLanguage2Tag(): string | null {
    return this.getValue(this.language2TagKey);
  }

  setLanguage2Tag(language2Tag: string): void {
    this.setValue(this.language2TagKey, language2Tag);
  }

  private getValue(key: string): string | null {
    const storage = this.getStorage();

    if (!storage) {
      return null;
    }

    return storage.getItem(key);
  }

  private setValue(key: string, value: string): void {
    const storage = this.getStorage();

    if (!storage) {
      return;
    }

    storage.setItem(key, value);
  }

  private getStorage(): Storage | null {
    if (typeof window === 'undefined' || !window.localStorage) {
      return null;
    }

    return window.localStorage;
  }
}
