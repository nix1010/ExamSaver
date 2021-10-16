import { getErrorResponseMessage } from 'src/app/utils/utils';
import { HttpErrorResponse } from '@angular/common/http';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from './../../../config/constants';
import { ExamService } from './../../../services/exam.service';
import { Component, OnInit } from '@angular/core';
import { Exam } from 'src/app/models/exam.model';
import { ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';
import { Role } from 'src/app/models/role.model';
import { start } from 'repl';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-exam-list',
    templateUrl: './exam-list.component.html',
    styleUrls: ['./exam-list.component.scss']
})
export class ExamListComponent implements OnInit {
    public exams: Exam[] = [];
    private role: Role;

    public showContent: boolean = false;
    public errorMessage: string = null;
    public showSpinner: boolean = false;
    public showErrorPage: boolean = false;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    Role = Role;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute
    ) { }

    ngOnInit(): void {
        let roles: Role[] = this.activatedRoute.snapshot.data.roles;

        this.role = roles[0];

        this.getExams();
    }

    getExams(): void {
        this.showSpinner = true;
        this.errorMessage = null;
        this.showContent = false;

        if (this.role === Role.PROFESSOR) {
            this.examService.getHoldingExams()
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe((exams: Exam[]) => {
                    this.exams = exams;
                    this.showContent = true;
                },
                    (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
        }
        else {
            this.examService.getTakingExams()
                .pipe(finalize(() => this.showSpinner = false))
                .subscribe((exams: Exam[]) => {
                    this.exams = exams;
                    this.showContent = true;
                },
                    (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
        }
    }

    getExamUri(exam: Exam): string {
        if (this.role === Role.PROFESSOR) {
            return `/exams/${exam.id}/students`;
        }
        else {
            return `/exams/${exam.id}/submit`;
        }
    }
}
