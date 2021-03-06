import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
import { StudentExam } from 'src/app/models/student-exam.model';
import { Exam } from "../models/exam.model";
import { FileInfo } from '../models/file-info.model';
import { FileContent } from './../models/file-content.model';

@Injectable()
export class ExamService {

    constructor(private httpClient: HttpClient) { }

    submitExam(examId: number, formData: FormData): Observable<any> {
        return this.httpClient.post(`exams/taking/${examId}`, formData, { reportProgress: true, observe: 'events' });
    }

    addExam(exam: Exam): Observable<any> {
        return this.httpClient.post(`exams/holding`, exam);
    }

    updateExam(examId: number, exam: Exam): Observable<any> {
        return this.httpClient.put(`exams/holding/${examId}`, exam);
    }

    deleteExam(examId: number): Observable<any> {
        return this.httpClient.delete(`exams/holding/${examId}`);
    }

    getTakingExams(pageQueryParam: number = null): Observable<HttpResponse<Exam[]>> {
        return this.httpClient.get<Exam[]>(this.appendPageQueryParam(`exams/taking`, pageQueryParam), { observe: 'response' });
    }

    getTakingExamById(examId: number): Observable<Exam> {
        return this.httpClient.get<Exam>(`exams/taking/${examId}`);
    }

    getHoldingExams(pageQueryParam: number = null): Observable<HttpResponse<Exam[]>> {
        return this.httpClient.get<Exam[]>(this.appendPageQueryParam(`exams/holding`, pageQueryParam), { observe: 'response' });
    }

    getHoldingExamById(examId: number): Observable<Exam> {
        return this.httpClient.get<Exam>(`exams/holding/${examId}`);
    }

    getStudentExams(examId: number): Observable<StudentExam[]> {
        return this.httpClient.get<StudentExam[]>(`exams/holding/${examId}/students`);
    }

    getStudentExam(examId: number, studentId: number): Observable<StudentExam> {
        return this.httpClient.get<StudentExam>(`exams/holding/${examId}/students/${studentId}`);
    }

    downloadStudentExam(examId: number, studentId: number): Observable<HttpResponse<Blob>> {
        return this.httpClient.get(`exams/holding/${examId}/students/${studentId}/download`, { responseType: 'blob', observe: 'response' });
    }

    getStudentExamFileTree(examId: number, studentId: number, fileTreePath: string): Observable<FileInfo[]> {
        return this.httpClient.get<FileInfo[]>(`exams/holding/${examId}/students/${studentId}/tree/${fileTreePath}`);
    }

    getStudentExamFileContent(examId: number, studentId: number, fileTreePath: string): Observable<FileContent> {
        return this.httpClient.get<FileContent>(`exams/holding/${examId}/students/${studentId}/file/${fileTreePath}`);
    }

    appendPageQueryParam(url: string, pageQueryParam: number): string {
        if (pageQueryParam) {
            url = `${url}?page=${pageQueryParam}`;
        }

        return url;
    }
}