import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'info' | 'warning' | 'error';

export interface AppNotification {
  id: number;
  message: string;
  type: NotificationType;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notifications = signal<AppNotification[]>([]);

  private readonly defaultDurationMs = 3000;
  private readonly maxVisibleNotifications = 4;
  private nextId = 1;
  private readonly dismissTimers = new Map<number, ReturnType<typeof setTimeout>>();

  success(message: string, durationMs = this.defaultDurationMs): void {
    this.show(message, 'success', durationMs);
  }

  info(message: string, durationMs = this.defaultDurationMs): void {
    this.show(message, 'info', durationMs);
  }

  warning(message: string, durationMs = this.defaultDurationMs): void {
    this.show(message, 'warning', durationMs);
  }

  error(message: string, durationMs = this.defaultDurationMs): void {
    this.show(message, 'error', durationMs);
  }

  remove(id: number): void {
    this.clearDismissTimer(id);
    this.notifications.update((items) => items.filter((item) => item.id !== id));
  }

  clearAll(): void {
    for (const timerId of this.dismissTimers.values()) {
      clearTimeout(timerId);
    }

    this.dismissTimers.clear();
    this.notifications.set([]);
  }

  private show(message: string, type: NotificationType, durationMs: number): void {
    const normalizedMessage = message.trim();

    if (!normalizedMessage) {
      return;
    }

    const safeDurationMs = Math.max(0, durationMs);

    const notification: AppNotification = {
      id: this.nextId++,
      message: normalizedMessage,
      type,
      durationMs: safeDurationMs,
    };

    this.notifications.update((items) => {
      const updatedItems = [...items, notification].slice(-this.maxVisibleNotifications);
      const visibleIds = new Set(updatedItems.map((item) => item.id));

      for (const item of items) {
        if (!visibleIds.has(item.id)) {
          this.clearDismissTimer(item.id);
        }
      }

      return updatedItems;
    });

    if (safeDurationMs > 0) {
      const timerId = setTimeout(() => {
        this.remove(notification.id);
      }, safeDurationMs);

      this.dismissTimers.set(notification.id, timerId);
    }
  }

  private clearDismissTimer(id: number): void {
    const timerId = this.dismissTimers.get(id);

    if (!timerId) {
      return;
    }

    clearTimeout(timerId);
    this.dismissTimers.delete(id);
  }
}
