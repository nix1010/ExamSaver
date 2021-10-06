import { HttpErrorResponse } from '@angular/common/http';

export function getErrorResponseMessage(err: HttpErrorResponse): string {
    if (err.status === 0) {
        return "Can't reach server right now, please try again later";
    }

    let message: string = err.error.message;

    if (message !== null && message !== undefined) {
        return message;
    }

    let title: string = err.error.title;

    if (title !== null && title !== undefined) {
        return title;
    }

    return "Unknown error, please try again later";
}