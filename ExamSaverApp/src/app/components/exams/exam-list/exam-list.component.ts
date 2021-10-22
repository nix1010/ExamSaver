import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { Role } from 'src/app/models/role.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from './../../../config/constants';
import { ExamService } from './../../../services/exam.service';

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

    getTitle(): string {
        if (this.role === Role.PROFESSOR) {
            return 'Holding exams';
        }

        return 'Active exams';
    }

    getExams(): void {
        this.showSpinner = true;
        this.errorMessage = null;
        this.showContent = false;

        this.getExamsByRole()
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((exams: Exam[]) => {
                this.exams = exams;
                this.showContent = true;
            },
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    getExamsByRole(): Observable<Exam[]> {
        if (this.role === Role.PROFESSOR) {
            return this.examService.getHoldingExams();
        }

        return this.examService.getTakingExams();
    }

    getExamUri(exam: Exam): string {
        if (this.role === Role.PROFESSOR) {
            return `/exams/holding/${exam.id}/students`;
        }
        else {
            return `/exams/taking/${exam.id}/submit`;
        }
    }
}
