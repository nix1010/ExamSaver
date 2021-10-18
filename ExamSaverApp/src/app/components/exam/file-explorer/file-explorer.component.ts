import { StudentExam } from 'src/app/models/student-exam.model';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { forkJoin, Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from 'src/app/config/constants';
import { getErrorResponseMessage, getFormattedFileSize } from 'src/app/utils/utils';
import { FileInfo } from './../../../models/file-info.model';
import { ExamService } from './../../../services/exam.service';
import { Exam } from 'src/app/models/exam.model';

@Component({
    selector: 'app-file-explorer',
    templateUrl: './file-explorer.component.html',
    styleUrls: ['./file-explorer.component.scss']
})
export class FileExplorerComponent implements OnInit, OnDestroy {
    public exam: Exam = null;
    public studentExam: StudentExam = null;
    public fileTree: FileInfo[] = [];
    private examId: number;
    private studentId: number;

    private baseFileTreePath: string;

    public showErrorPage: boolean = false;
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    private studentExamSubscription: Subscription = null;
    private routerEventsSubscription: Subscription = null;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;

    getFormattedFileSize = getFormattedFileSize;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit(): void {
        let examId = this.activatedRoute.snapshot.paramMap.get('examId');
        let studentId = this.activatedRoute.snapshot.paramMap.get('studentId');

        this.examId = Number(examId);
        this.studentId = Number(studentId);

        if (Number.isNaN(this.examId) || Number.isNaN(this.studentId)) {
            this.showErrorPage = true;
        }
        else {
            this.baseFileTreePath = `/exams/holding/${examId}/students/${studentId}/tree/`;

            this.routerEventsSubscription = this.router.events.subscribe((event: NavigationEnd) => {
                if (event instanceof NavigationEnd) {
                    this.getStudentExamFileTree(this.examId, this.studentId);
                }
            });

            this.showSpinner = true;

            forkJoin([
                this.examService.getHoldingExamById(this.examId),
                this.examService.getStudentExam(this.examId, this.studentId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, studentExam]) => {
                    this.exam = exam;
                    this.studentExam = studentExam;
                    this.getStudentExamFileTree(this.examId, this.studentId);
                }, (error: HttpErrorResponse) => {
                    this.errorMessage = getErrorResponseMessage(error);
                    this.showErrorPage = true;
                });
        }
    }

    ngOnDestroy(): void {
        this.unsubscribeFrom(this.routerEventsSubscription);
        this.unsubscribeFrom(this.studentExamSubscription);
    }

    getStudentExamFileTree(examId: number, studentId: number): void {
        this.showSpinner = true;
        this.showErrorPage = false;
        this.showContent = false;
        this.errorMessage = null;

        let pathRegex: RegExp = new RegExp(`${this.baseFileTreePath}?`);
        let fileTreePath: string = this.router.url.replace(pathRegex, '');

        this.unsubscribeFrom(this.studentExamSubscription);

        this.studentExamSubscription = this.examService.getStudentExamFileTree(examId, studentId, fileTreePath)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((fileTree: FileInfo[]) => {
                this.fileTree = fileTree;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
                this.showErrorPage = true;
            });
    }

    getParentDirectoryRoute(): string {
        let url: string = this.router.url;
        let index: number = url.lastIndexOf('/');

        return url.substring(0, index);
    }

    isRoot(): boolean {
        return this.router.url === this.baseFileTreePath.slice(0, -1);
    }

    unsubscribeFrom(subscription: Subscription): void {
        if (subscription !== null) {
            subscription.unsubscribe();
        }
    }
}
