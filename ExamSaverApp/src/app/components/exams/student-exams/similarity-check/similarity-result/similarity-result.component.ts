import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { DISPLAY_DATE_FORMAT } from 'src/app/config/constants';
import { SimilarityResult } from 'src/app/models/similarity-result.model';
import { SimilarityService } from 'src/app/services/similarity.service';
import { getErrorResponseMessage } from 'src/app/utils/utils';

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

    deleteSimilarityResult(similarityResult: SimilarityResult): void {
        let confirmation: boolean = confirm("Do you really want to delete?");

        if (!confirmation) {
            return;
        }

        this.deleting = true;
        
        this.similarityService.deleteSimilarityResult(this.examId, similarityResult.id)
            .pipe(finalize(() => this.deleting = false))
            .subscribe(() => {
                this.deletedEmitter.emit();
            }, (error: HttpErrorResponse) => this.errorEmitter.emit(getErrorResponseMessage(error)));
    }
}
