import { Subject } from './../../../models/subject.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { INPUT_DATE_TIME_FORMAT } from './../../../config/constants';
import { HttpErrorResponse } from '@angular/common/http';
import { ExamService } from './../../../services/exam.service';
import { SubjectService } from './../../../services/subject.service';
import { Component, Input, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Exam } from 'src/app/models/exam.model';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-create-exam',
    templateUrl: './add-update-exam.component.html',
    styleUrls: ['./add-update-exam.component.scss']
})
export class AddUpdateExamComponent implements OnInit {

    public exam: Exam = new Exam();
    public subjects: Subject[] = [];
    public examId: number;
    public update: boolean = false;

    public showErrorPage: boolean = false;
    public showContent: boolean = false;
    public showSpinner: boolean = false;
    public errorMessage: string = null;
    public submitProcess: boolean = false;
    public submitProcessSuccess: boolean = false;

    @ViewChild('form')
    public formElement: ElementRef<any>;

    public DATE_FORMAT = INPUT_DATE_TIME_FORMAT;

    constructor(
        private activatedRoute: ActivatedRoute,
        public subjectService: SubjectService,
        private examService: ExamService
    ) { }

    ngOnInit(): void {
        this.getSubjects();

        let examIdParam = this.activatedRoute.snapshot.paramMap.get('examId');

        this.update = examIdParam !== null;

        if (this.update) {
            this.examId = Number(examIdParam);

            if (Number.isNaN(this.examId)) {
                this.showErrorPage = true;
            } else {
                this.getExamForUpdate(this.examId);
            }
        }
    }

    getSubjects(): void {
        this.subjectService.getTeachingSubjects()
            .subscribe((subjects: Subject[]) => this.subjects = subjects,
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    getExamForUpdate(examId: number): void {
        this.showSpinner = true;
        this.showErrorPage = this.showContent = false;

        this.examService.getHoldingExamById(examId)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((exam: Exam) => {
                this.exam = exam;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.showErrorPage = true;
                this.errorMessage = getErrorResponseMessage(error);
            });
    }

    addUpdateExam(): void {
        this.errorMessage = null;
        this.submitProcess = true;
        this.submitProcessSuccess = false;

        if (this.validateForm()) {
            if (this.update) {
                this.examService.updateExam(this.examId, this.exam)
                    .pipe(finalize(() => this.submitProcess = false))
                    .subscribe(_response => this.submitProcessSuccess = true,
                        (err: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(err));
            }
            else {
                this.examService.addExam(this.exam)
                    .pipe(finalize(() => this.submitProcess = false))
                    .subscribe(_response => this.submitProcessSuccess = true,
                        (err: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(err));
            }
        }
        else {
            this.submitProcess = false;
        }
    }

    validateForm(): boolean {
        let validated: boolean = true;

        if (!this.exam.startTime || !this.exam.endTime || !this.exam.endTime || !this.exam.subjectId) {
            validated = false;
        }

        this.formElement.nativeElement.classList.add('was-validated');

        return validated;
    }
}
