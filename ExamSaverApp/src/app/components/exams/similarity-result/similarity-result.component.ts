import { finalize } from 'rxjs/operators';
import { SimilarityService } from './../../../services/similarity.service';
import { DISPLAY_DATE_FORMAT } from './../../../config/constants';
import { SimilarityResult } from '../../../models/similarity-result.model';
import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-similarity-result',
    templateUrl: './similarity-result.component.html',
    styleUrls: ['./similarity-result.component.scss']
})
export class SimilarityResultComponent implements OnInit {
    public deleting: boolean = false;

    @Input() similarityResult: SimilarityResult;
    @Input() examId: number;

    @Output('deleted') deletedEmitter: EventEmitter<void> = new EventEmitter<void>();
    @Output('error') errorEmitter: EventEmitter<string> = new EventEmitter<string>();

    DISPLAY_DATE_TIME_FORMAT = DISPLAY_DATE_FORMAT;

    constructor(private similarityService: SimilarityService) { }

    ngOnInit(): void {
    }

    deleteSimilarityResult(similarityResultId: number): void {
        let confirmation: boolean = confirm("Do you really want to delete?");

        if (!confirmation) {
            return;
        }

        this.deleting = true;
        
        this.similarityService.deleteSimilarityResult(this.examId, similarityResultId)
            .pipe(finalize(() => this.deleting = false))
            .subscribe(() => {
                this.deletedEmitter.emit();
            }, (error: HttpErrorResponse) => this.errorEmitter.emit(getErrorResponseMessage(error)));
    }
}
