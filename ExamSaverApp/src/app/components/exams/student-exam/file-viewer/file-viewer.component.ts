import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { ExamService } from 'src/app/services/exam.service';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { getErrorResponseMessage, getFormattedFileSize } from 'src/app/utils/utils';
import { FileContent } from './../../../../models/file-content.model';


@Component({
    selector: 'app-file-viewer',
    templateUrl: './file-viewer.component.html',
    styleUrls: ['./file-viewer.component.scss']
})
export class FileViewerComponent implements OnInit {
    public file: FileContent = null;

    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    getFormattedFileSize = getFormattedFileSize;

    constructor(
        private examService: ExamService,
        private router: Router,
        private studentExamService: StudentExamService
    ) { }

    ngOnInit(): void {
        let examId: number = this.studentExamService.studentExam.examId;
        let studentId: number = this.studentExamService.studentExam.studentId;

        this.getStudentExamFileContent(examId, studentId);
    }

    getStudentExamFileContent(examId: number, studentId: number): void {
        this.showSpinner = true;
        this.showContent = false;
        this.errorMessage = null;

        let pathRegex: RegExp = new RegExp(`${this.studentExamService.studentExamFileContentUri}?`);
        let fileTreePath: string = this.router.url.replace(pathRegex, '');

        this.examService.getStudentExamFileContent(examId, studentId, fileTreePath)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((file: FileContent) => {
                this.file = file;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
            });
    }
}
