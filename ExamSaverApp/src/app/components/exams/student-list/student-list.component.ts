import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { MossResult } from 'src/app/models/moss-result.model';
import { MossRunResult } from 'src/app/models/moss-run-result.model';
import { StudentExam } from 'src/app/models/student-exam.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_DATE_TIME_FORMAT, DISPLAY_TIME_FORMAT, FILE_EXTENSIONS, ID_NOT_VALID_MESSAGE } from './../../../config/constants';
import { MossRequest } from './../../../models/moss-request.model';
import { ExamService } from './../../../services/exam.service';
import { MossService } from './../../../services/moss.service';

@Component({
    selector: 'app-student-list',
    templateUrl: './student-list.component.html',
    styleUrls: ['./student-list.component.scss'],
    providers: [MossService]
})
export class StudentListComponent implements OnInit {
    public exam: Exam = null;
    private examId: number;
    public examStudents: StudentExam[] = [];
    public mossResults: MossResult[] = [];
    public mossRequest: MossRequest = new MossRequest();
    public mossRunResult: MossRunResult = null;

    @ViewChild('form')
    public formElement: ElementRef<any>;

    public showMossRunningSpinner: boolean = false;
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    FILE_EXTENSIONS = FILE_EXTENSIONS;
    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    DISPLAY_DATE_TIME_FORMAT = DISPLAY_DATE_TIME_FORMAT;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute,
        private mossService: MossService
    ) { }

    ngOnInit(): void {
        let examIdParam = this.activatedRoute.snapshot.paramMap.get('examId');
        this.examId = Number(examIdParam);

        if (Number.isNaN(this.examId)) {
            this.errorMessage = ID_NOT_VALID_MESSAGE;
        }
        else {
            this.showSpinner = true;
            this.showContent = false;

            forkJoin([
                this.examService.getHoldingExamById(this.examId),
                this.examService.getExamStudents(this.examId),
                this.mossService.getMossResults(this.examId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, examStudents, mossResults]) => {
                    this.exam = exam;
                    this.examStudents = examStudents;
                    this.mossResults = mossResults;
                    this.showContent = true;
                }, (error: HttpErrorResponse) => {
                    this.errorMessage = getErrorResponseMessage(error);
                });
        }
    }

    performSimilarityCheck(): void {
        if (!this.validateFileExtensionForm()) {
            return;
        }

        this.showMossRunningSpinner = true;
        this.errorMessage = null;
        this.mossRunResult = null;

        this.mossService.runSimilarityCheck(this.examId, this.mossRequest)
            .pipe(finalize(() => this.showMossRunningSpinner = false))
            .subscribe((mossRunResult: MossRunResult) => {
                this.showMossRunningSpinner = true;
                this.mossRunResult = mossRunResult;

                this.mossService.getMossResults(this.examId)
                    .pipe(finalize(() => this.showMossRunningSpinner = false))
                    .subscribe((mossResults: MossResult[]) => this.mossResults = mossResults,
                        (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
            },
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    deleteSimilarityResult(mossResultId: number): void {
        let confirmation: boolean = confirm("Do you really want to delete?");

        if (!confirmation) {
            return;
        }

        this.mossService.deleteMossResult(this.examId, mossResultId)
            .subscribe(() => {
                this.mossService.getMossResults(this.examId)
                    .subscribe((mossResults: MossResult[]) => this.mossResults = mossResults,
                        (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
            }, (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    validateFileExtensionForm(): boolean {
        let valid: boolean = true;

        if (!this.mossRequest.fileExtension) {
            valid = false;
        }

        this.formElement.nativeElement.classList.add('was-validated');

        return valid;
    }
}
