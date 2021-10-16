import { ExamService } from './../../../services/exam.service';
import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { getErrorResponseMessage } from 'src/app/utils/utils';

@Component({
    selector: 'app-file-upload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent implements OnInit {
    
    public examId: number;
    public progress: number = 0;
    public errorMessage: string = null;
    public fileSetForUpload: boolean = false;
    public uploadInProgress: boolean = false;
    public uploadSuccess: boolean = false;
    public file: File;

    @ViewChild("fileInput")
    private fileInput: ElementRef;

    constructor(private examService: ExamService) { }

    ngOnInit(): void {
    }

    onFormClick() {
        this.fileInput.nativeElement.value = '';
        this.fileInput.nativeElement.click();
    }

    getFormattedFileSize(): string {
        const kilobyte = 1000;
        const bytes = this.file.size;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
        const sizeIndex = Math.floor(Math.log(bytes) / Math.log(kilobyte));
        const convertedBytes = bytes / Math.pow(kilobyte, sizeIndex);

        return `${Math.round((convertedBytes + Number.EPSILON) * 100) / 100} ${sizes[sizeIndex]}`;
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

        this.examService.submitExamFile(this.examId, formData)
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