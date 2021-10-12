import { getErrorResponseMessage } from 'src/app/utils/utils';
import { DATE_FORMAT } from './../../../config/constants';
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
    public update: boolean = false;
    public showError: boolean = false;
    public showContent: boolean = false;
    public showSpinner: boolean = false;
    public examId: number;
    public errorMessage: string = null;
    public submitProcess: boolean = false;
    public submitProcessSuccess: boolean = false;

    @ViewChild('form')
    public formElement: ElementRef<any>;

    public DATE_FORMAT = DATE_FORMAT;

    constructor(
        private route: ActivatedRoute,
        public subjectService: SubjectService,
        private examService: ExamService
    ) { }

    ngOnInit(): void {
        let examId = this.route.snapshot.paramMap.get('examId');

        this.showContent = true;

        if (examId !== null) {
            this.update = true;
            this.examId = Number(examId);

            if (Number.isNaN(this.examId)) {
                this.showError = true;
            } else {
                this.showSpinner = true;
                this.showError = this.showContent = false;

                this.examService.getExamById(this.examId)
                    .pipe(finalize(() => this.showSpinner = false))
                    .subscribe((exam: Exam) => {
                        this.exam = exam;
                        this.showContent = true;
                    }, (_error: HttpErrorResponse) => {
                        this.showError = true;
                    });
            }
        }
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
