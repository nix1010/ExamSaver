import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { StudentExam } from 'src/app/models/student-exam.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { SimilarityService } from '../../../services/similarity.service';
import { DISPLAY_DATE_FORMAT, DISPLAY_DATE_TIME_FORMAT, DISPLAY_TIME_FORMAT, ID_NOT_VALID_MESSAGE } from '../../../config/constants';
import { ExamService } from '../../../services/exam.service';

@Component({
    selector: 'app-student-exams',
    templateUrl: './student-exams.component.html',
    styleUrls: ['./student-exams.component.scss'],
    providers: [SimilarityService]
})
export class StudentExamsComponent implements OnInit {
    public exam: Exam = null;
    public examId: number;
    public examStudents: StudentExam[] = [];

    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    public similarityRunMessage: string = null;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    DISPLAY_DATE_TIME_FORMAT = DISPLAY_DATE_TIME_FORMAT;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute
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
                this.examService.getStudentExams(this.examId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, examStudents]) => {
                    this.exam = exam;
                    this.examStudents = examStudents;
                    this.showContent = true;
                }, (error: HttpErrorResponse) => {
                    this.errorMessage = getErrorResponseMessage(error);
                });
        }
    }

    onSimilarityCheckRun(): void {
        this.similarityRunMessage = this.errorMessage = null;
    }

    onSimilarityCheckResult(runMessage: string): void {
        this.similarityRunMessage = runMessage;
    }

    onSimilarityCheckError(error: HttpErrorResponse): void {
        console.log(1);
        this.errorMessage = getErrorResponseMessage(error);
    }
}
