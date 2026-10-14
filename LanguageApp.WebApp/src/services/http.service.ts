import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { TranslationDto, TranslationUpdateDto } from '../models/models';
import { NotificationService } from './notification.service';
import { backendUrl } from "../environment/environment";

@Injectable({ providedIn: 'root' })
export class TranslationHttpService {
  private backendUrl: string;
  // private readonly getNextTranslationEndpoint = 'http://localhost:8080/api/get-next-translation';
  // private readonly updateTranslationEndpoint = 'http://localhost:8080/api/update-translation';
  private readonly notificationService = inject(NotificationService);


  constructor(
    private readonly http: HttpClient
  ) {
    this.backendUrl = backendUrl;
  }


  getNextTranslation(
    userEmail: string,
    language1Tag: string,
    language2Tag: string,
  ): Observable<TranslationDto> {
    const params = new HttpParams()
      .set('userEmail', userEmail)
      .set('language1Tag', language1Tag)
      .set('language2Tag', language2Tag);
    return this.http
      .get<TranslationDto>(`${this.backendUrl}/api/get-next-translation`, { params })
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  updateTranslation(translationUpdate: TranslationUpdateDto): Observable<void> {
    return this.http
      .patch<void>(`${this.backendUrl}/api/update-translation`, translationUpdate)
      .pipe(catchError((errorResponse: HttpErrorResponse) => this.handleHttpError(errorResponse)));
  }


  private handleHttpError(errorResponse: HttpErrorResponse): Observable<never> {
    this.notificationService.error(errorResponse.error.error);
    console.log(errorResponse.error.error);
    return throwError(() => errorResponse);
  }
}
