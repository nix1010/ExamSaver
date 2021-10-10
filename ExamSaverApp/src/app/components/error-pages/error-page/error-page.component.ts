import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-error-page',
    templateUrl: './error-page.component.html',
    styleUrls: ['./error-page.component.scss']
})
export class ErrorPageComponent implements OnInit {

    public errorCode: string = null;
    private static NOT_FOUND: string = '404';

    constructor(private location: Location) {
        const state: any = this.location.getState();

        if (state && state.errorCode) {
            this.errorCode = state.errorCode;
        }

        if (this.errorCode === null) {
            this.errorCode = ErrorPageComponent.NOT_FOUND;
        }
    }

    ngOnInit(): void {
    }

    getMessage() {
        switch (this.errorCode) {
            case ErrorPageComponent.NOT_FOUND: return 'The page you are looking for was not found.';
            default: return 'Unknown error occurred';
        }
    }

}
