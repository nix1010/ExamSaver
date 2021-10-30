import { Component, Input, OnInit } from '@angular/core';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT } from 'src/app/config/constants';
import { Exam } from 'src/app/models/exam.model';
import { Role } from 'src/app/models/role.model';
import { getExamsUri } from 'src/app/utils/utils';

@Component({
    selector: 'app-exam-list',
    templateUrl: './exam-list.component.html',
    styleUrls: ['./exam-list.component.scss']
})
export class ExamListComponent implements OnInit {
    @Input() public exams: Exam[];
    @Input() public role: Role;

    Role = Role;

    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;

    constructor() { }

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
