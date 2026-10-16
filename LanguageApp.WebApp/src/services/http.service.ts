import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { TranslationBatchItemDto, TranslationUpdateDto, UserSettingsDto } from '../models/models';
import { NotificationService } from './notification.service';
import { backendUrl } from "../environment/environment";

@Injectable({ providedIn: 'root' })
export class TranslationHttpService {
  private backendUrl: string;
  // private readonly updateTranslationEndpoint = 'http://localhost:8080/api/update-translation';
  private readonly notificationService = inject(NotificationService);


  constructor(
    private readonly http: HttpClient
  ) {
    this.backendUrl = backendUrl;
  }


  getTranslationBatch(
    userEmail: string,
    language1Tag: string,
    language2Tag: string,
  ): Observable<TranslationBatchItemDto[]> {
    const params = new HttpParams()
      .set('userEmail', userEmail)
      .set('language1Tag', language1Tag)
      .set('language2Tag', language2Tag);
    return this.http
      .get<TranslationBatchItemDto[]>(`${this.backendUrl}/api/get-translation-batch`, { params })
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  updateTranslation(translationUpdate: TranslationUpdateDto): Observable<void> {
    return this.http
      .patch<void>(`${this.backendUrl}/api/update-translation`, translationUpdate)
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  getUserSettings(userEmail: string): Observable<UserSettingsDto> {
    const params = new HttpParams().set('userEmail', userEmail);
    return this.http
      .get<UserSettingsDto>(`${this.backendUrl}/api/user-settings`, { params })
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  saveUserSettings(userEmail: string, settings: UserSettingsDto): Observable<UserSettingsDto> {
    const params = new HttpParams().set('userEmail', userEmail);
    return this.http
      .put<UserSettingsDto>(`${this.backendUrl}/api/user-settings`, settings, { params })
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  private handleHttpError(errorResponse: HttpErrorResponse): Observable<never> {
    const message = errorResponse.error?.error ?? errorResponse.message;
    this.notificationService.error(message);
    console.log(message);
    return throwError(() => errorResponse);
  }
}
