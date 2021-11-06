import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { SimilarityResult } from 'src/app/models/similarity-result.model';
import { SimilarityRunResult } from 'src/app/models/similarity-run-result.model';
import { StudentExam } from 'src/app/models/student-exam.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_DATE_TIME_FORMAT, DISPLAY_TIME_FORMAT, FILE_EXTENSIONS, ID_NOT_VALID_MESSAGE } from './../../../config/constants';
import { SimilarityRequest } from '../../../models/similarity-request.model';
import { ExamService } from './../../../services/exam.service';
import { SimilarityService } from '../../../services/similarity.service';

@Component({
    selector: 'app-student-list',
    templateUrl: './student-list.component.html',
    styleUrls: ['./student-list.component.scss'],
    providers: [SimilarityService]
})
export class StudentListComponent implements OnInit {
    public exam: Exam = null;
    public examId: number;
    public examStudents: StudentExam[] = [];
    public similarityResults: SimilarityResult[] = [];
    public similarityRequest: SimilarityRequest = new SimilarityRequest();
    public similarityRunResult: SimilarityRunResult = null;

    @ViewChild('form')
    public formElement: ElementRef;

    public showSimilarityRunningSpinner: boolean = false;
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
        private similarityService: SimilarityService
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
                this.similarityService.getSimilarityResults(this.examId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, examStudents, similarityResults]) => {
                    this.exam = exam;
                    this.examStudents = examStudents;
                    this.similarityResults = similarityResults;
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

        this.showSimilarityRunningSpinner = true;
        this.errorMessage = null;
        this.similarityRunResult = null;

        this.similarityService.runSimilarityCheck(this.examId, this.similarityRequest)
            .pipe(finalize(() => this.showSimilarityRunningSpinner = false))
            .subscribe((similarityRunResult: SimilarityRunResult) => {
                this.showSimilarityRunningSpinner = true;
                this.similarityRunResult = similarityRunResult;

                this.similarityService.getSimilarityResults(this.examId)
                    .pipe(finalize(() => this.showSimilarityRunningSpinner = false))
                    .subscribe((similarityResults: SimilarityResult[]) => this.similarityResults = similarityResults,
                        (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
            },
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    deletedSimilarityResult(): void {
        this.similarityService.getSimilarityResults(this.examId)
            .subscribe((similarityResults: SimilarityResult[]) => this.similarityResults = similarityResults,
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    validateFileExtensionForm(): boolean {
        let valid: boolean = true;

        if (!this.similarityRequest.fileExtension) {
            valid = false;
        }

        this.formElement.nativeElement.classList.add('was-validated');

        return valid;
    }
}
