import { SimilarityRequest } from '../models/similarity-request.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { SimilarityResult } from '../models/similarity-result.model';
import { SimilarityRunResult } from '../models/similarity-run-result.model';

@Injectable()
export class SimilarityService {

    constructor(private httpClient: HttpClient) { }

    getSimilarityResults(examId: number): Observable<SimilarityResult[]> {
        return this.httpClient.get<SimilarityResult[]>(`exams/holding/${examId}/students/similarity`);
    }

    deleteSimilarityResult(examId: number, similarityResultId: number): Observable<any> {
        return this.httpClient.delete(`exams/holding/${examId}/students/similarity/${similarityResultId}`);
    }

    runSimilarityCheck(examId: number, similarityRequest: SimilarityRequest): Observable<SimilarityRunResult> {
        return this.httpClient.post<SimilarityRunResult>(`exams/holding/${examId}/students/similarity`, similarityRequest);
    }
}