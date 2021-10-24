import { Role } from './../models/role.model';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';

export function getErrorResponseMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
        return "Can't reach server right now, please try again later";
    }

    if (error.error && error.error.message) {
        return error.error.message;
    }

    return 'Unknown error';
}

export function getFormattedFileSize(fileSize: number): string {
    const kilobyte = 1000;
    const bytes = fileSize;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    const sizeIndex = Math.floor(Math.log(bytes) / Math.log(kilobyte));
    const convertedBytes = bytes / Math.pow(kilobyte, sizeIndex);

    return `${Math.round((convertedBytes + Number.EPSILON) * 100) / 100} ${sizes[sizeIndex]}`;
}

export function unsubscribeFrom(subscription: Subscription): void {
    if (subscription) {
        subscription.unsubscribe();
    }
}

export function getExamsUri(role: Role): string {
    if (role === Role.PROFESSOR) {
        return `/exams/holding`;
    }
    else {
        return `/exams/taking`;
    }
}