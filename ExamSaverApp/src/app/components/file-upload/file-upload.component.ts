import { HttpEvent } from '@angular/common/http';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';

@Component({
    selector: 'app-file-upload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent implements OnInit {
    public progress: number = 0;
    public errorMessage: string = null;
    public fileSetForUpload: boolean = false;
    public uploadInProgress: boolean = false;
    public uploadSuccess: boolean = false;
    public file: File;

    @ViewChild("fileInput")
    private fileInput: ElementRef;

    constructor(private httpClient: HttpClient) { }

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
        const formData: FormData = new FormData();


        for (let i = 0; i < fileList.length; ++i) {
            this.file = fileList[i];
            formData.append(fileList[i].name, fileList[i], fileList[i].name);
        }

        this.httpClient.post('exams/1', formData, { reportProgress: true, observe: 'events' })
            .subscribe((event: HttpEvent<Object>) => {
                if (event.type === HttpEventType.UploadProgress)
                    this.progress = Math.round(100 * event.loaded / event.total);
                else if (event.type === HttpEventType.Response) {
                    this.uploadSuccess = true;
                    this.uploadInProgress = false;


                }
            }, error => {
                this.errorMessage = getErrorResponseMessage(error);
                console.log(this.errorMessage);
            });
    }
}