import { Injectable } from '@angular/core';
import { StudentExam } from './../models/student-exam.model';

@Injectable()
export class StudentExamService {
    private _studentExam: StudentExam = null;
    private _studentExamUri: string = null;
    private _studentExamFileTreeUri: string;
    private _studentExamFileContentUri: string;

    get studentExam(): StudentExam {
        return this._studentExam;
    }

    get studentExamUri(): string {
        return this._studentExamUri;
    }

    get studentExamFileTreeUri(): string {
        return this._studentExamFileTreeUri;
    }

    get studentExamFileContentUri(): string {
        return this._studentExamFileContentUri;
    }

    set studentExam(studentExam: StudentExam) {
        this._studentExam = studentExam;
        this._studentExamUri = `/exams/holding/${this._studentExam.examId}/students/${this._studentExam.studentId}`;
        this._studentExamFileTreeUri = `${this.studentExamUri}/tree/`;
        this._studentExamFileContentUri = `${this.studentExamUri}/file/`;
    }
}