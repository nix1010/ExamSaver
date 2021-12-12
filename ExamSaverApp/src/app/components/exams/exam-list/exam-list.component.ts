import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Exam } from 'src/app/models/exam.model';
import { Role } from 'src/app/models/role.model';

@Component({
    selector: 'app-exam-list',
    templateUrl: './exam-list.component.html',
    styleUrls: ['./exam-list.component.scss']
})
export class ExamListComponent implements OnInit {
    @Input() public exams: Exam[];
    @Input() public role: Role;

    @Output('deleted') deletedEmitter: EventEmitter<void> = new EventEmitter<void>();
    @Output('error') errorEmitter: EventEmitter<HttpErrorResponse> = new EventEmitter<HttpErrorResponse>();

    constructor() { }

    ngOnInit(): void {
    }
}
