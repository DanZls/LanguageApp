import { Injectable } from '@angular/core';

type AudioPayload = string | number[] | null;

@Injectable({ providedIn: 'root' })
export class AudioService {
  private readonly audioPlayer = new Audio();

  hasAudio(audioData: AudioPayload): boolean {
    if (Array.isArray(audioData)) {
      return audioData.length > 0;
    }

    return Boolean(audioData?.trim());
  }

  playBase64Audio(audioData: AudioPayload, loudness = 1): void {
    const audioSource = this.buildAudioSource(audioData);

    if (!audioSource) {
      return;
    }

    this.audioPlayer.pause();
    this.audioPlayer.currentTime = 0;
    this.audioPlayer.volume = this.normalizeLoudness(loudness);
    this.audioPlayer.src = audioSource;

    void this.audioPlayer.play().catch(() => {
      // Browser blocked autoplay or audio format could not be decoded.
    });
  }

  private buildAudioSource(audioData: AudioPayload): string | null {
    if (audioData == null) {
      return null;
    }

    const audioBase64 = Array.isArray(audioData) ? this.bytesToBase64(audioData) : audioData.trim();

    if (!audioBase64) {
      return null;
    }

    if (audioBase64.startsWith('data:audio/')) {
      return audioBase64;
    }

    return `data:${this.detectAudioMimeType(audioBase64)};base64,${audioBase64}`;
  }

  private bytesToBase64(bytes: number[]): string {
    if (bytes.length === 0) {
      return '';
    }

    const binaryString = String.fromCharCode(...bytes);
    return btoa(binaryString);
  }

  private normalizeLoudness(loudness: number): number {
    if (!Number.isFinite(loudness)) {
      return 1;
    }

    return Math.min(1, Math.max(0, loudness));
  }

  private detectAudioMimeType(base64: string): string {
    try {
      const decoded = atob(base64.slice(0, 32));

      if (decoded.startsWith('RIFF')) {
        return 'audio/wav';
      }

      if (decoded.startsWith('ID3')) {
        return 'audio/mpeg';
      }

      if (decoded.startsWith('OggS')) {
        return 'audio/ogg';
      }
    } catch {
      // Keep default below when base64 is malformed.
    }

    return 'audio/mpeg';
  }
}
