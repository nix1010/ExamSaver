import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { ID_NOT_VALID_MESSAGE, INPUT_DATE_TIME_FORMAT } from './../../../config/constants';
import { Subject } from './../../../models/subject.model';
import { ExamService } from './../../../services/exam.service';
import { SubjectService } from './../../../services/subject.service';

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
                this.errorMessage = ID_NOT_VALID_MESSAGE;
            } else {
                this.getExamForUpdate(this.examId);
            }
        }
        else {
            this.showContent = true;
        }
    }

    getSubjects(): void {
        this.subjectService.getTeachingSubjects()
            .subscribe((subjects: Subject[]) => this.subjects = subjects,
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    getExamForUpdate(examId: number): void {
        this.showSpinner = true;
        this.showContent = false;

        this.examService.getHoldingExamById(examId)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((exam: Exam) => {
                this.exam = exam;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
            });
    }

    addUpdateExam(): void {
        if (this.validateForm()) {
            this.errorMessage = null;
            this.submitProcess = true;
            this.submitProcessSuccess = false;

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
