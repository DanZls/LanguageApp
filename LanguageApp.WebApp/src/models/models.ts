export interface TranslationDto {
  translationId: number;
  translationLearningInfoId: number;
  term: string | null;
  termMeaning: string | null;
  termTranslation: string | null;
  termTranslationTags: string[] | null;
  termTranslationAudio: number[] | null;
  isTranslationFlagged: boolean | null;
  isTranslationSkipped: boolean | null;
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
