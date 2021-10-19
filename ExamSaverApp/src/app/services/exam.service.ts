import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
import { StudentExam } from 'src/app/models/student-exam.model';
import { Exam } from "../models/exam.model";
import { FileInfo } from '../models/file-info.model';
import { File } from '../models/file.model';

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

    getTakingExams(): Observable<Exam[]> {
        return this.httpClient.get<Exam[]>(`exams/taking`);
    }

    getTakingExamById(examId: number): Observable<Exam> {
        return this.httpClient.get<Exam>(`exams/taking/${examId}`);
    }

    getHoldingExams(): Observable<Exam[]> {
        return this.httpClient.get<Exam[]>(`exams/holding`);
    }

    getHoldingExamById(examId: number): Observable<Exam> {
        return this.httpClient.get<Exam>(`exams/holding/${examId}`);
    }

    getExamStudents(examId: number): Observable<StudentExam[]> {
        return this.httpClient.get<StudentExam[]>(`exams/holding/${examId}/students`);
    }

    getStudentExam(examId: number, studentId: number): Observable<StudentExam> {
        return this.httpClient.get<StudentExam>(`exams/holding/${examId}/students/${studentId}`);
    }

    downloadExam(examId: number, studentId: number): Observable<HttpResponse<Blob>> {
        return this.httpClient.get(`exams/holding/${examId}/students/${studentId}/download`, { responseType: 'blob', observe: 'response' });
    }

    getStudentExamFileTree(examId: number, studentId: number, fileTreePath: string): Observable<FileInfo[]> {
        return this.httpClient.get<FileInfo[]>(`exams/holding/${examId}/students/${studentId}/tree/${fileTreePath}`);
    }

    getStudentExamFile(examId: number, studentId: number, fileTreePath: string): Observable<File[]> {
        return this.httpClient.get<File[]>(`exams/holding/${examId}/students/${studentId}/file/${fileTreePath}`);
    }
}