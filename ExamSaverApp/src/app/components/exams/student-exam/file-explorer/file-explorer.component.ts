import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { StudentExamService } from 'src/app/services/student-exam.service';
import { equalUris, getErrorResponseMessage, getFormattedFileSize, unsubscribeFrom } from 'src/app/utils/utils';
import { FileInfo } from '../../../../models/file-info.model';
import { ExamService } from '../../../../services/exam.service';

@Component({
    selector: 'app-file-explorer',
    templateUrl: './file-explorer.component.html',
    styleUrls: ['./file-explorer.component.scss']
})
export class FileExplorerComponent implements OnInit, OnDestroy {
    public fileTree: FileInfo[] = [];

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
        unsubscribeFrom(this.routerEventsSubscription);
        unsubscribeFrom(this.studentExamFileTreeSubscription);
    }

    getStudentExamFileTree(examId: number, studentId: number): void {
        this.showSpinner = true;
        this.showContent = false;
        this.errorMessage = null;

        let pathRegex: RegExp = new RegExp(`${this.studentExamService.studentExamFileTreeUri}?`);
        let fileTreePath: string = this.router.url.replace(pathRegex, '');

        unsubscribeFrom(this.studentExamFileTreeSubscription);

        this.studentExamFileTreeSubscription = this.examService.getStudentExamFileTree(examId, studentId, fileTreePath)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((fileTree: FileInfo[]) => {
                this.fileTree = fileTree;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
            });
    }

    getFileUri(file: FileInfo): string {
        if (file.isDirectory) {
            return this.studentExamService.studentExamFileTreeUri + file.fullPath;
        }

        return this.studentExamService.studentExamFileContentUri + file.fullPath;
    }

    getParentDirectoryUri(): string {
        let url: string = this.router.url;
        let index: number = url.lastIndexOf('/');

        return url.substring(0, index);
    }

    isRoot(): boolean {
        return equalUris(this.router.url, this.studentExamService.studentExamFileTreeUri);
    }
}
