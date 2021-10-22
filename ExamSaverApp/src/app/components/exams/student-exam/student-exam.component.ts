import { HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from 'src/app/config/constants';
import { Exam } from 'src/app/models/exam.model';
import { ExamService } from 'src/app/services/exam.service';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { getErrorResponseMessage } from 'src/app/utils/utils';

@Component({
    selector: 'app-student-exam',
    templateUrl: './student-exam.component.html',
    styleUrls: ['./student-exam.component.scss'],
    providers: [StudentExamService]
})
export class StudentExamComponent implements OnInit {
    public exam: Exam = null;

    public showErrorPage: boolean = false;
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute,
        private studentExamService: StudentExamService
    ) { }

    ngOnInit(): void {
        let examIdParam = this.activatedRoute.snapshot.paramMap.get('examId');
        let studentIdParam = this.activatedRoute.snapshot.paramMap.get('studentId');

        let examId: number = Number(examIdParam);
        let studentId: number = Number(studentIdParam);

        if (Number.isNaN(examId) || Number.isNaN(studentId)) {
            this.showErrorPage = true;
        }
        else {
            this.showSpinner = true;
            
            forkJoin([
                this.examService.getHoldingExamById(examId),
                this.examService.getStudentExam(examId, studentId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, studentExam]) => {
                    this.exam = exam;
                    this.studentExamService.studentExam = studentExam;
                    this.showContent = true;
                }, (error: HttpErrorResponse) => {
                    this.errorMessage = getErrorResponseMessage(error);
                    this.showErrorPage = true;
                });
        }
    }

    downloadExam(): void {
        let examId: number = this.studentExamService.studentExam.examId;
        let studentId: number = this.studentExamService.studentExam.studentId;

        this.examService.downloadStudentExam(examId, studentId)
            .subscribe((response: HttpResponse<Blob>) => {
                const fileName = this.getFilenameFromContentDisposition(response.headers);
                const blobUrl = URL.createObjectURL(response.body);

                const a = document.createElement('a');
                a.href = blobUrl
                a.download = fileName;
                a.click();

                URL.revokeObjectURL(blobUrl);
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
            });
    }

    private getFilenameFromContentDisposition(headers: HttpHeaders): string {
        let fileName: string = "archive.zip";
        const contentDisposition: string = headers.get("Content-Disposition");

        if (contentDisposition !== null) {
            const regex: RegExp = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/g;
            const match: RegExpExecArray = regex.exec(contentDisposition);

            if (match.length > 1) {
                fileName = match[1];
            }
        }

        return fileName;
    }

}
