import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LocalStorageService {
  private readonly userEmailKey = 'language-app.user-email';

  getUserEmail(): string | null {
    return this.getValue(this.userEmailKey);
  }

  setUserEmail(userEmail: string): void {
    this.setValue(this.userEmailKey, userEmail);
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
