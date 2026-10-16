import { TranslationBatchItemDto } from '../../models/models';

/**
 * Local copy of the batch served by the backend. It mirrors the server-side selection rule:
 * the next word is the least recently viewed one (never viewed first), then the most frequent one.
 */
export class VocabularyBatch {
  private items: TranslationBatchItemDto[] = [];
  // Logical "now" for local views: always later than any stored LastViewAt and strictly increasing
  private viewClock = 0;

  get isEmpty(): boolean {
    return this.items.length === 0;
  }

  replace(items: TranslationBatchItemDto[]): void {
    this.items = [...items];
    this.viewClock = items.reduce((max, item) => Math.max(max, this.viewTime(item)), 0);
    this.sort();
  }

  /** The word to show next, or undefined when the batch is exhausted. */
  next(): TranslationBatchItemDto | undefined {
    this.sort();
    return this.items[0];
  }

  /** The word was viewed: it stays in the batch but moves behind the words not viewed yet. */
  markViewed(item: TranslationBatchItemDto): void {
    this.viewClock = Math.max(Date.now(), this.viewClock + 1);
    item.lastViewAt = new Date(this.viewClock).toISOString();
    this.sort();
  }

  remove(item: TranslationBatchItemDto): void {
    this.items = this.items.filter((candidate) => candidate !== item);
  }

  private sort(): void {
    this.items.sort(
      (a, b) => this.viewTime(a) - this.viewTime(b) || a.frequencyIndex - b.frequencyIndex,
    );
  }

  private viewTime(item: TranslationBatchItemDto): number {
    return item.lastViewAt === null ? Number.NEGATIVE_INFINITY : Date.parse(item.lastViewAt);
  }
}
