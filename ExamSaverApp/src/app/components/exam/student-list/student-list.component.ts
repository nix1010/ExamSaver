import { getErrorResponseMessage } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from './../../../config/constants';
import { ExamService } from './../../../services/exam.service';
import { Component, OnInit } from '@angular/core';
import { Exam } from 'src/app/models/exam.model';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { StudentExam } from 'src/app/models/student-exam.model';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-student-list',
    templateUrl: './student-list.component.html',
    styleUrls: ['./student-list.component.scss']
})
export class StudentListComponent implements OnInit {
    public exam: Exam = null;
    private examId: number;
    public examStudents: StudentExam[] = [];

    public showErrorPage: boolean = false;
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute
    ) { }

    ngOnInit(): void {
        let examId = this.activatedRoute.snapshot.paramMap.get('examId');
        this.examId = Number(examId);

        if (Number.isNaN(this.examId)) {
            this.showErrorPage = true;
        }
        else {
            this.showSpinner = true;
            this.showErrorPage = this.showContent = false;

            forkJoin([
                this.examService.getHoldingExamById(this.examId),
                this.examService.getExamStudents(this.examId)
            ])
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe(([exam, examStudents]) => {
                    this.exam = exam;
                    this.examStudents = examStudents;
                    this.showContent = true;
                }, (error: HttpErrorResponse) => {
                    this.errorMessage = getErrorResponseMessage(error);
                    this.showErrorPage = true;
                });
        }
    }
}
