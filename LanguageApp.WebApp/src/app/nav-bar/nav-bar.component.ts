import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-nav-bar',
  standalone: true,
  templateUrl: './nav-bar.component.html',
  styleUrl: './nav-bar.component.css',
})
export class NavBarComponent {
  @Input() activeLink = 'Vocabulary';
  @Output() activeLinkChange = new EventEmitter<string>();

  readonly links = [{ label: 'Grammar' }, { label: 'Vocabulary' }, { label: 'About' }];

  selectLink(label: string, event: Event): void {
    event.preventDefault();
    this.activeLinkChange.emit(label);
  }

  openAccount(): void {
    this.activeLinkChange.emit('Settings');
  }
}
