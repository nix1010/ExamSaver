import { HttpClient } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
import { Exam } from "../models/exam.model";

@Injectable()
export class ExamService {

    constructor(private httpClient: HttpClient) { }

    getExamById(examId: number): Observable<Exam> {
        return this.httpClient.get<Exam>(`exams/holding/${examId}`);
    }

    addExam(exam: Exam): Observable<any> {
        return this.httpClient.post(`exams/add`, exam);
    }

    updateExam(examId: number, exam: Exam): Observable<any> {
        return this.httpClient.put(`exams/${examId}/update`, exam);
    }
}