export interface TranslationDto {
  translationId: number;
  translationLearningInfoId: number;
  term: string | null;
  termMeaning: string | null;
  termTranslation: string | null;
  alternativeForms: string | null;
  termTranslationTags: string[] | null;
  termTranslationAudio: number[] | null;
  isTranslationFlagged: boolean | null;
  isTranslationSkipped: boolean | null;
}


export interface TranslationBatchItemDto extends TranslationDto {
  frequencyIndex: number;
  lastViewAt: string | null;
}


export class TranslationUpdateDto {
  translationId: number = 0;
  translationLearningInfoId: number = 0;
  isTranslationFlagged: boolean | null = null;
  isTranslationSkipped: boolean | null = null;
  isViewed: boolean | null = null;
  isLearned: boolean | null = null;

  constructor(init?: Partial<TranslationUpdateDto>) {
    Object.assign(this, init);
  }
}


export type AppTheme = 'light' | 'dark';


export interface UserSettingsDto {
  autoPlayAudio: boolean;
  audioLoudness: number;
  language1Tag: string;
  language2Tag: string;
  vocabularyBatchSize: number;
  theme: AppTheme;
}
