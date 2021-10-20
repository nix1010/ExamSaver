import { DISPLAY_TIME_FORMAT, DISPLAY_DATE_FORMAT } from '../../../config/constants';
import { ExamService } from '../../../services/exam.service';
import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { getErrorResponseMessage, getFormattedFileSize } from 'src/app/utils/utils';
import { Exam } from 'src/app/models/exam.model';
import { finalize } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

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
    public showErrorPage: boolean = false;


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
            this.showErrorPage = true;
        } else {
            this.getExamForSubmit(this.examId);
        }
    }

    getExamForSubmit(examId: number): void {
        this.showSpinner = true;
        this.showErrorPage = this.showContent = false;

        this.examService.getTakingExamById(examId)
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((exam: Exam) => {
                this.exam = exam;
                this.showContent = true;
            }, (error: HttpErrorResponse) => {
                this.showErrorPage = true;
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