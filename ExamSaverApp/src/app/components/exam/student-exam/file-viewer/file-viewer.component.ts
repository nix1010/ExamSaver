import { Router } from '@angular/router';
import { FileContent } from './../../../../models/file-content.model';
import { Component, OnInit } from '@angular/core';
import { getFormattedFileSize, getErrorResponseMessage } from 'src/app/utils/utils';
import { ExamService } from 'src/app/services/exam.service';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { finalize } from 'rxjs/operators';
import { HttpResponse, HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-file-viewer',
    templateUrl: './file-viewer.component.html',
    styleUrls: ['./file-viewer.component.scss']
})
export class FileViewerComponent implements OnInit {
    public fileContent: FileContent = null;

    public showErrorPage: boolean = false;
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
        this.showErrorPage = false;
        this.showContent = false;
        this.errorMessage = null;

        let pathRegex: RegExp = new RegExp(`${this.studentExamService.studentExamFileContentUri}?`);
        let fileTreePath: string = this.router.url.replace(pathRegex, '');

        this.examService.getStudentExamFileContent(examId, studentId, fileTreePath)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((fileContent: FileContent) => {
                this.fileContent = fileContent;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
                this.showErrorPage = true;
            });
    }
}
