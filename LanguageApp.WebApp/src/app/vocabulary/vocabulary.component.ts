import { Component, OnInit, inject, signal } from '@angular/core';

import { EMPTY, firstValueFrom, Observable } from 'rxjs';
import { TranslationDto, TranslationUpdateDto } from '../../models/models';
import { AudioService } from '../../services/audio.service';
import { TranslationHttpService } from '../../services/http.service';
import { NotificationService } from '../../services/notification.service';
import { UserSettingsService } from '../../services/user-settings.service';

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
    await this.loadNextTranslation();
    this.isTermVisible.set(true);
  }

  async markLearned(): Promise<void> {
    this.isTermVisible.set(false);
    this.isTranslationVisible.set(false);
    await firstValueFrom(
      this.updateTranslation(new TranslationUpdateDto({ isViewed: true, isLearned: true }))
    );
    await this.loadNextTranslation();
    this.isTermVisible.set(true);
  }

  async switchFlagged(): Promise<void> {
    await firstValueFrom(
      this.updateTranslation(new TranslationUpdateDto({
        isTranslationFlagged: !this.translation().isTranslationFlagged
      }))
    );
    this.loadNextTranslation();
  }

  switchSkipped(): void {
    this.isTranslationVisible.set(false);
    this.updateTranslation(new TranslationUpdateDto({
      isTranslationSkipped: !this.translation().isTranslationSkipped
    })).subscribe({
      next: () => this.loadNextTranslation(),
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
    await firstValueFrom(
      this.translationHttpService.getNextTranslation(
        userEmail,
        this.userSettings.language1Tag(),
        this.userSettings.language2Tag(),
      )
    )
    .then((translation: TranslationDto) => {
      this.translation.set(translation);
    })
    .catch(() => {
      this.notificationService.error('Failed to load next translation', 5000);
    });
    return;
  }

  private updateTranslation(translationUpdate: TranslationUpdateDto): Observable<void> {
    translationUpdate.translationId = this.translation().translationId;
    translationUpdate.translationLearningInfoId = this.translation().translationLearningInfoId;
    return this.translationHttpService.updateTranslation(translationUpdate);
  }
}
