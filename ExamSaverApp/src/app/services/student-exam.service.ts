import { Injectable } from '@angular/core';
import { StudentExam } from './../models/student-exam.model';

@Injectable()
export class StudentExamService {
    private _studentExam: StudentExam = null;
    private _studentExamUri: string = null;
    private _baseFileTreePath: string;
    private _baseFileContentPath: string;

    get studentExam(): StudentExam {
        return this._studentExam;
    }

    get studentExamUri(): string {
        return this._studentExamUri;
    }

    get baseFileTreePath(): string {
        return this._baseFileTreePath;
    }

    get baseFileContentPath(): string {
        return this._baseFileContentPath;
    }

    set studentExam(studentExam: StudentExam) {
        this._studentExam = studentExam;
        this._studentExamUri = `/exams/holding/${this._studentExam.examId}/students/${this._studentExam.studentId}`;
        this._baseFileTreePath = `${this.studentExamUri}/tree/`;
        this._baseFileContentPath = `${this.studentExamUri}/file/`;
    }
}