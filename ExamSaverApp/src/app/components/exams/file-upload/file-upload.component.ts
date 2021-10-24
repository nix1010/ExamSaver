import { HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { getErrorResponseMessage, getFormattedFileSize } from 'src/app/utils/utils';
import { DISPLAY_DATE_FORMAT, DISPLAY_TIME_FORMAT, ID_NOT_VALID_MESSAGE } from '../../../config/constants';
import { ExamService } from '../../../services/exam.service';

@Component({
    selector: 'app-file-upload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent implements OnInit {
    public exam: Exam = null;
    public examId: number;

    public progress: number = 0;
    public fileSetForUpload: boolean = false;
    public uploadInProgress: boolean = false;
    public uploadSuccess: boolean = false;
    public file: File;

    @ViewChild("fileInput")
    private fileInput: ElementRef;

    public errorMessage: string = null;
    public showSpinner: boolean = false;
    public showContent: boolean = false;

    DISPLAY_TIME_FORMAT = DISPLAY_TIME_FORMAT;
    DISPLAY_DATE_FORMAT = DISPLAY_DATE_FORMAT;
    
    getFormattedFileSize = getFormattedFileSize;

    constructor(
        private examService: ExamService,
        private activatedRoute: ActivatedRoute
    ) { }

    ngOnInit(): void {
        let examId = this.activatedRoute.snapshot.paramMap.get('examId');

        this.examId = Number(examId);

        if (Number.isNaN(this.examId)) {
            this.errorMessage = ID_NOT_VALID_MESSAGE;
        } else {
            this.getExamForSubmit(this.examId);
        }
    }

    getExamForSubmit(examId: number): void {
        this.showSpinner = true;
        this.showContent = false;
        this.errorMessage = null;

        this.examService.getTakingExamById(examId)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((exam: Exam) => {
                this.exam = exam;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(error);
            });
    }

    onFormClick() {
        this.fileInput.nativeElement.value = '';
        this.fileInput.nativeElement.click();
    }

    uploadFile(fileList: FileList): void {
        if (fileList.length == 0) {
            return;
        }

        this.progress = 0;
        this.fileSetForUpload = true;
        this.uploadInProgress = true;
        this.uploadSuccess = false;
        this.errorMessage = null;

        const formData: FormData = new FormData();

        this.file = fileList[0];
        formData.append(this.file.name, this.file, this.file.name);

        this.examService.submitExam(this.examId, formData)
            .subscribe((event: HttpEvent<Object>) => {
                if (event.type === HttpEventType.UploadProgress) {
                    this.progress = Math.round(100 * event.loaded / event.total);
                }
                else if (event.type === HttpEventType.Response) {
                    this.uploadInProgress = false;
                    this.uploadSuccess = true;
                }
            }, (error: HttpErrorResponse) => {
                this.uploadInProgress = false;
                this.errorMessage = getErrorResponseMessage(error);
            });
    }
}