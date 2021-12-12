import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { SimilarityRunResult } from 'src/app/models/similarity-run-result.model';
import { SimilarityRequest } from 'src/app/models/similarity-request.model';
import { SimilarityResult } from 'src/app/models/similarity-result.model';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { FILE_EXTENSIONS } from 'src/app/config/constants';
import { SimilarityService } from 'src/app/services/similarity.service';

@Component({
    selector: 'app-similarity-check',
    templateUrl: './similarity-check.component.html',
    styleUrls: ['./similarity-check.component.scss']
})
export class SimilarityCheckComponent implements OnInit {
    @Input() examId: number;
    @Input() show: boolean;

    @Output('run') runEmitter: EventEmitter<void> = new EventEmitter<void>();
    @Output('result') resultEmitter: EventEmitter<string> = new EventEmitter<string>();
    @Output('error') errorEmitter: EventEmitter<HttpErrorResponse> = new EventEmitter<HttpErrorResponse>();

    @ViewChild('form')
    public formElement: ElementRef<any>;
    
    public showSimilarityRunningSpinner: boolean = false;
    public similarityResults: SimilarityResult[] = [];
    public similarityRequest: SimilarityRequest = new SimilarityRequest();

    FILE_EXTENSIONS = FILE_EXTENSIONS;
    
    constructor(
        private similarityService: SimilarityService
    ) { }

    ngOnInit(): void {
        this.similarityService.getSimilarityResults(this.examId)
    }

    performSimilarityCheck(): void {
        this.runEmitter.emit();

        if (!this.validateFileExtensionForm()) {
            return;
        }

        this.showSimilarityRunningSpinner = true;

        this.similarityService.runSimilarityCheck(this.examId, this.similarityRequest)
            .pipe(finalize(() => this.showSimilarityRunningSpinner = false))
            .subscribe((similarityRunResult: SimilarityRunResult) => {
                this.showSimilarityRunningSpinner = true;

                this.resultEmitter.emit(similarityRunResult.runMessage);

                this.similarityService.getSimilarityResults(this.examId)
                    .pipe(finalize(() => this.showSimilarityRunningSpinner = false))
                    .subscribe((similarityResults: SimilarityResult[]) => this.similarityResults = similarityResults,
                        (error: HttpErrorResponse) => this.errorEmitter.emit(error));
            },
                (error: HttpErrorResponse) => this.errorEmitter.emit(error));
    }

    deletedSimilarityResult(): void {
        this.similarityService.getSimilarityResults(this.examId)
            .subscribe((similarityResults: SimilarityResult[]) => this.similarityResults = similarityResults,
                (error: HttpErrorResponse) => this.errorEmitter.emit(error));
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
