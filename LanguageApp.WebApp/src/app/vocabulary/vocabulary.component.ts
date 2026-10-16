import { Component, OnInit, inject, signal } from '@angular/core';

import { EMPTY, firstValueFrom, Observable } from 'rxjs';
import { TranslationBatchItemDto, TranslationDto, TranslationUpdateDto } from '../../models/models';
import { AudioService } from '../../services/audio.service';
import { TranslationHttpService } from '../../services/http.service';
import { NotificationService } from '../../services/notification.service';
import { UserSettingsService } from '../../services/user-settings.service';
import { VocabularyBatch } from './vocabulary-batch';

@Component({
  selector: 'app-vocabulary',
  standalone: true,
  templateUrl: './vocabulary.component.html',
  styleUrl: './vocabulary.component.css',
})
export class VocabularyComponent implements OnInit {
  readonly userSettings = inject(UserSettingsService);

  private readonly translationHttpService = inject(TranslationHttpService);
  private readonly audioService = inject(AudioService);
  private readonly notificationService = inject(NotificationService);

  private readonly emptyTranslation: TranslationDto = {
    translationId: 0,
    translationLearningInfoId: 0,
    term: '',
    termMeaning: '',
    termTranslation: '',
    termTranslationTags: [],
    termTranslationAudio: null,
    isTranslationFlagged: false,
    isTranslationSkipped: false,
  };

  readonly translation = signal<TranslationDto>(this.emptyTranslation);

  // Words are served in batches; the batch is walked locally and refilled from the backend when empty
  private readonly batch = new VocabularyBatch();
  private currentItem: TranslationBatchItemDto | undefined;

  readonly isTermVisible = signal(false);
  readonly isTranslationVisible = signal(false);


  ngOnInit(): void {
    this.loadNextTranslation()
    .then(() => {
      this.isTermVisible.set(true)
    });
  }

  revealTranslation(): void {
    this.isTranslationVisible.set(true);
    if (this.userSettings.autoPlayAudio()) {
      this.playTranslationAudio();
    }
  }

  async markViewed(): Promise<void> {
    this.isTermVisible.set(false);
    this.isTranslationVisible.set(false);
    await firstValueFrom(
      this.updateTranslation(new TranslationUpdateDto({ isViewed: true }))
    );
    this.applyViewed();
    await this.loadNextTranslation();
    this.isTermVisible.set(true);
  }

  async markLearned(): Promise<void> {
    this.isTermVisible.set(false);
    this.isTranslationVisible.set(false);
    await firstValueFrom(
      this.updateTranslation(new TranslationUpdateDto({ isViewed: true, isLearned: true }))
    );
    if (this.currentItem) {
      this.batch.remove(this.currentItem);
    }
    await this.loadNextTranslation();
    this.isTermVisible.set(true);
  }

  async switchFlagged(): Promise<void> {
    const isTranslationFlagged = !this.translation().isTranslationFlagged;
    await firstValueFrom(
      this.updateTranslation(new TranslationUpdateDto({ isTranslationFlagged }))
    );
    // Flagging does not change which word is next; just refresh the current one
    if (this.currentItem) {
      this.currentItem.isTranslationFlagged = isTranslationFlagged;
      this.translation.set({ ...this.currentItem });
    }
  }

  switchSkipped(): void {
    this.isTranslationVisible.set(false);
    const isTranslationSkipped = !this.translation().isTranslationSkipped;
    this.updateTranslation(new TranslationUpdateDto({ isTranslationSkipped })).subscribe({
      next: () => {
        if (this.currentItem) {
          this.currentItem.isTranslationSkipped = isTranslationSkipped;
          if (isTranslationSkipped) {
            this.batch.remove(this.currentItem);
          }
        }
        this.loadNextTranslation();
      },
    });
  }

  hasTranslationAudio(): boolean {
    return this.audioService.hasAudio(this.translation().termTranslationAudio);
  }

  playTranslationAudio(): void {
    if (!this.hasTranslationAudio()) {
      this.notificationService.warning('No audio available for this translation.', 2200);
      return;
    }

    this.audioService.playBase64Audio(
      this.translation().termTranslationAudio,
      this.userSettings.audioLoudness(),
    );
  }

  private async loadNextTranslation(): Promise<void> {
    const userEmail = this.userSettings.userEmail();
    if (!userEmail) {
      this.notificationService.warning('Please set your account email in Settings.', 5000);
      return;
    }
    if (this.batch.isEmpty) {
      const loaded = await this.loadBatch(userEmail);
      if (!loaded) {
        return;
      }
    }

    const next = this.batch.next();
    if (!next) {
      this.notificationService.error('Failed to load next translation', 5000);
      return;
    }
    this.currentItem = next;
    this.translation.set({ ...next });
  }

  private async loadBatch(userEmail: string): Promise<boolean> {
    try {
      const items = await firstValueFrom(
        this.translationHttpService.getTranslationBatch(
          userEmail,
          this.userSettings.language1Tag(),
          this.userSettings.language2Tag(),
        )
      );
      this.batch.replace(items);
      return true;
    } catch {
      this.notificationService.error('Failed to load next translation', 5000);
      return false;
    }
  }

  private applyViewed(): void {
    if (this.currentItem) {
      this.batch.markViewed(this.currentItem);
    }
  }

  private updateTranslation(translationUpdate: TranslationUpdateDto): Observable<void> {
    translationUpdate.translationId = this.translation().translationId;
    translationUpdate.translationLearningInfoId = this.translation().translationLearningInfoId;
    return this.translationHttpService.updateTranslation(translationUpdate);
  }
}
