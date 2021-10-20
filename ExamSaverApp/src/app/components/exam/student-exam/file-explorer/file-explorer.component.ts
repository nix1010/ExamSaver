import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { getErrorResponseMessage, getFormattedFileSize } from 'src/app/utils/utils';
import { FileInfo } from '../../../../models/file-info.model';
import { ExamService } from '../../../../services/exam.service';

@Component({
    selector: 'app-file-explorer',
    templateUrl: './file-explorer.component.html',
    styleUrls: ['./file-explorer.component.scss']
})
export class FileExplorerComponent implements OnInit, OnDestroy {
    public fileTree: FileInfo[] = [];

    public showErrorPage: boolean = false;
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    private studentExamFileTreeSubscription: Subscription = null;
    private routerEventsSubscription: Subscription = null;

    getFormattedFileSize = getFormattedFileSize;

    constructor(
        private examService: ExamService,
        private router: Router,
        private studentExamService: StudentExamService
    ) { }

    ngOnInit(): void {
        let examId: number = this.studentExamService.studentExam.examId;
        let studentId: number = this.studentExamService.studentExam.studentId;

        this.routerEventsSubscription = this.router.events.subscribe((event: NavigationEnd) => {
            if (event instanceof NavigationEnd) {
                this.getStudentExamFileTree(examId, studentId);
            }
        });

        this.getStudentExamFileTree(examId, studentId);
    }

    ngOnDestroy(): void {
        this.unsubscribeFrom(this.routerEventsSubscription);
        this.unsubscribeFrom(this.studentExamFileTreeSubscription);
    }

    getStudentExamFileTree(examId: number, studentId: number): void {
        this.showSpinner = true;
        this.showErrorPage = false;
        this.showContent = false;
        this.errorMessage = null;

        let pathRegex: RegExp = new RegExp(`${this.studentExamService.baseFileTreePath}?`);
        let fileTreePath: string = this.router.url.replace(pathRegex, '');

        this.unsubscribeFrom(this.studentExamFileTreeSubscription);
        
        this.studentExamFileTreeSubscription = this.examService.getStudentExamFileTree(examId, studentId, fileTreePath)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((fileTree: FileInfo[]) => {
                this.fileTree = fileTree;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
                this.showErrorPage = true;
            });
    }

    getFileUri(file: FileInfo): string {
        if (file.isDirectory) {
            return this.studentExamService.baseFileTreePath + file.fullPath;
        }

        return this.studentExamService.baseFileContentPath + file.fullPath;
    }

    getParentDirectoryUri(): string {
        let url: string = this.router.url;
        let index: number = url.lastIndexOf('/');

        return url.substring(0, index);
    }

    isRoot(): boolean {
        return this.router.url === this.studentExamService.baseFileTreePath.slice(0, -1);
    }

    unsubscribeFrom(subscription: Subscription): void {
        if (subscription !== null) {
            subscription.unsubscribe();
        }
    }
}
