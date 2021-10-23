import { Component, Host, Input, OnInit } from '@angular/core';
import { Exam } from 'src/app/models/exam.model';
import { Role } from 'src/app/models/role.model';
import { getExamsUri } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from './../../../config/constants';
import { ExamsComponent } from './../exams.component';

@Component({
    selector: 'app-exam-list',
    templateUrl: './exam-list.component.html',
    styleUrls: ['./exam-list.component.scss']
})
export class ExamListComponent implements OnInit {
    @Input() public exams: Exam[];
    @Input() public role: Role;

    public showContent: boolean = false;
    public errorMessage: string = null;
    public showSpinner: boolean = false;
    public showErrorPage: boolean = false;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    Role = Role;

    constructor(@Host() private host: ExamsComponent) { }

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
}
