import { HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT, ID_NOT_VALID_MESSAGE } from 'src/app/config/constants';
import { Exam } from 'src/app/models/exam.model';
import { ExamService } from 'src/app/services/exam.service';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { equalUris, getErrorResponseMessage } from 'src/app/utils/utils';

@Component({
    selector: 'app-student-exam',
    templateUrl: './student-exam.component.html',
    styleUrls: ['./student-exam.component.scss'],
    providers: [StudentExamService]
})
export class StudentExamComponent implements OnInit {
    public exam: Exam = null;

    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;

    constructor(
        private router: Router,
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
            this.errorMessage = ID_NOT_VALID_MESSAGE;
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
                });
        }
    }

    downloadExam(): void {
        let examId: number = this.studentExamService.studentExam.examId;
        let studentId: number = this.studentExamService.studentExam.studentId;

        this.errorMessage = null;

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
                if (error.status === 404) {
                    this.errorMessage = 'Resource file is not found';
                }
                else {
                    this.errorMessage = getErrorResponseMessage(error);
                }
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

    getRootUri(): string {
        return this.studentExamService.studentExamFileTreeUri;
    }

    isRoot(): boolean {
        return equalUris(this.router.url, this.getRootUri());
    }

    getFileTreeLevelsUri(): string[] {
        let fileTreeUriSegments: string[] = this.getFileTreeUriSegments(this.router.url);
        let fileTreeLevelsUri: string[] = [];
        let baseUri: string = this.studentExamService.studentExamFileTreeUri;

        for (let fileTreeUriSegment of fileTreeUriSegments) {
            let fileTreeLevelUri: string;

            if (fileTreeLevelsUri.length > 0) {
                fileTreeLevelUri = `${fileTreeLevelsUri[fileTreeLevelsUri.length - 1]}/${fileTreeUriSegment}`;
            }
            else {
                fileTreeLevelUri = `${baseUri}${fileTreeUriSegment}`;
            }

            fileTreeLevelsUri.push(fileTreeLevelUri);
        }

        return fileTreeLevelsUri;
    }

    private getFileTreeUriSegments(uri: string): string[] {
        let fileTreeUriSegments: string[] = [];
        let baseUri: string = this.studentExamService.studentExamFileTreeUri;
        let baseUriStartIndex: number = uri.indexOf(baseUri);

        if (baseUriStartIndex === -1) {
            baseUri = this.studentExamService.studentExamFileContentUri;
            baseUriStartIndex = uri.indexOf(baseUri);
        }

        if (baseUriStartIndex !== -1) {
            fileTreeUriSegments = uri.replace(baseUri, '').split('/');
        }

        return fileTreeUriSegments;
    }

    getLastUriSegment(uri: string): string {
        let fileTreeUriSegments: string[] = uri.split('/');

        if (fileTreeUriSegments.length > 0) {
            return fileTreeUriSegments[fileTreeUriSegments.length - 1];
        }

        return '';
    }
}
