import { HttpErrorResponse } from '@angular/common/http';

export function getErrorResponseMessage(err: HttpErrorResponse): string {
    if (err.status === 0) {
        return "Can't reach server right now, please try again later";
    }

    if (err.error) {
        return err.error.message;
    }

    return err.message;
}