import { ExamService } from 'src/app/services/exam.service';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from 'src/app/config/constants';
import { Exam } from 'src/app/models/exam.model';
import { Role } from 'src/app/models/role.model';
import { getExamsUri } from 'src/app/utils/utils';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-exam-item',
    templateUrl: './exam-item.component.html',
    styleUrls: ['./exam-item.component.scss']
})
export class ExamItemComponent implements OnInit {
    @Input() public exam: Exam;
    @Input() public role: Role;

    @Output('deleted') deletedEmitter: EventEmitter<void> = new EventEmitter<void>();
    @Output('error') errorEmitter: EventEmitter<HttpErrorResponse> = new EventEmitter<HttpErrorResponse>();

    public deleting: boolean = false;

    Role = Role;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    
    constructor(
        private examService: ExamService
    ) { }

    ngOnInit(): void {
    }

    getExamUri(exam: Exam): string {
        let examsUri: string = getExamsUri(this.role);

        if (this.role === Role.PROFESSOR) {
            return `${examsUri}/${exam.id}/students`;
        }
        else {
            return `${examsUri}/${exam.id}/submit`;
        }
    }

    deleteExam(exam: Exam): void {
        let confirmation: boolean = confirm("Do you really want to delete exam and all of its submitted work?");

        if (!confirmation) {
            return;
        }

        this.deleting = true;

        this.examService.deleteExam(exam.id)
            .pipe(finalize(() => this.deleting = false))
            .subscribe(() => this.deletedEmitter.emit(),
                (error: HttpErrorResponse) => this.errorEmitter.emit(error));
    }
}
