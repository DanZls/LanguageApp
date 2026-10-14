import { Component, computed, signal } from '@angular/core';
import { marked } from 'marked';

interface GrammarTopic {
  id: string;
  title: string;
  markdown: string;
}

@Component({
  selector: 'app-grammar',
  standalone: true,
  templateUrl: './grammar.component.html',
  styleUrl: './grammar.component.css',
})
export class GrammarComponent {
  readonly topics = signal<GrammarTopic[]>([
    {
      id: 'present-simple',
      title: 'Present Simple',
      markdown: `# Present Simple

Use **Present Simple** for habits, facts, and repeated actions.

## Structure
- I/You/We/They + base verb
- He/She/It + base verb + **s/es**

## Examples
- I work every day.
- She works in Berlin.

## Signal words
- always
- usually
- often
- every day`,
    },
    {
      id: 'past-simple',
      title: 'Past Simple',
      markdown: `# Past Simple

Use **Past Simple** for completed actions in the past.

## Structure
- Regular verbs: verb + **ed**
- Irregular verbs: second form

## Examples
- We visited Prague last year.
- He went home at 9 PM.`,
    },
    {
      id: 'articles',
      title: 'Articles (a / an / the)',
      markdown: `# Articles

Use **a/an** for non-specific singular nouns.
Use **the** for specific nouns.

## Quick rules
- a book
- an apple
- the sun

## Tip
Use \`the\` when both speaker and listener know the thing.`,
    },
  ]);

  readonly selectedTopicId = signal(this.topics()[0]?.id ?? '');

  readonly selectedTopic = computed(
    () => this.topics().find((topic) => topic.id === this.selectedTopicId()) ?? this.topics()[0],
  );

  readonly renderedTopicContent = computed(() => {
    const parsed = marked.parse(this.selectedTopic()?.markdown ?? '', {
      gfm: true,
      async: false,
    });

    return typeof parsed === 'string' ? parsed : '';
  });

  selectTopic(topicId: string): void {
    this.selectedTopicId.set(topicId);
  }

}
