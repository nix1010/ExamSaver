import { MossRequest } from './../models/moss-request.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { MossResult } from '../models/moss-result.model';

@Injectable()
export class MossService {

    constructor(private httpClient: HttpClient) { }

    getMossResults(examId: number): Observable<MossResult[]> {
        return this.httpClient.get<MossResult[]>(`exams/holding/${examId}/students/similarity`);
    }

    deleteMossResult(examId: number, mossResultId: number): Observable<any> {
        return this.httpClient.get(`exams/holding/${examId}/students/similarity/${mossResultId}`);
    }

    runSimilarityCheck(examId: number, mossRequest: MossRequest): Observable<any> {
        return this.httpClient.post(`exams/holding/${examId}/students/similarity`, mossRequest);
    }
}