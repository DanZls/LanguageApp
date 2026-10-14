import { Component, signal } from '@angular/core';
import { GrammarComponent } from './grammar/grammar.component';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { VocabularyComponent } from './vocabulary/vocabulary.component';
import { ToastComponent } from './toast/toast.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';

type TabName = 'Grammar' | 'Vocabulary' | 'Settings' | 'About';

@Component({
  selector: 'app-root',
  imports: [

    NavBarComponent,
    VocabularyComponent,
    GrammarComponent,
    UserSettingsComponent,
    ToastComponent,
  ],
  templateUrl: './app.html',

  styleUrl: './app.css',
})
export class App {
  activeTab = signal<TabName>('Vocabulary');

    selectTab(tab: string): void {
    if (tab === 'Grammar' || tab === 'Vocabulary' || tab === 'Settings' || tab === 'About') {
      this.activeTab.set(tab);
    }
  }

}
