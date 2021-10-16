import { HttpClient } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
import { Exam } from "../models/exam.model";
import { StudentExam } from '../models/student-exam.model';
import { File } from '../models/file.model';
import { FileInfo } from '../models/file-info.model';

@Injectable()
export class ExamService {

    constructor(private httpClient: HttpClient) { }

    addExam(exam: Exam): Observable<any> {
        return this.httpClient.post(`exams/add`, exam);
    }

    updateExam(examId: number, exam: Exam): Observable<any> {
        return this.httpClient.put(`exams/${examId}/update`, exam);
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

    getExamStudents(examId: number, studentId: number): Observable<StudentExam[]> {
        return this.httpClient.get<StudentExam[]>(`exams/${examId}/students`);
    }

    submitExamFile(examId: number, formData: FormData): Observable<any> {
        return this.httpClient.post(`exams/${examId}`, formData, { reportProgress: true, observe: 'events' });
    }

    getStudentExamFileTree(examId: number, studentId: number, fileTreePath: string): Observable<File[]> {
        return this.httpClient.get<File[]>(`exams/${examId}/students/${studentId}/tree/${fileTreePath}`);
    }

    getStudentExamFile(examId: number, studentId: number, fileTreePath: string): Observable<FileInfo[]> {
        return this.httpClient.get<FileInfo[]>(`exams/${examId}/students/${studentId}/file/${fileTreePath}`);
    }
}