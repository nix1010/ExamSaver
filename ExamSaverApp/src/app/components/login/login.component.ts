import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { User } from 'src/app/models/user-credentials.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { UserService } from './../../services/user.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
    public user: User = new User();
    public errorMessage: string = null;
    public loginProcess: boolean = false;

    @ViewChild('form')
    public formElement: ElementRef;

    constructor(
        private userService: UserService,
        private router: Router,
        private location: Location
    ) { }

    ngOnInit(): void {
        this.routeToPreviousPageIfAuthenticated();
    }

    authenticate() {
        if (!this.validateForm()) {
            return;
        }

        this.loginProcess = true;
        this.errorMessage = null;

        this.userService.authenticate(this.user)
            .pipe(finalize(() => this.loginProcess = false))
            .subscribe(() => {
                this.routeToPreviousPageIfAuthenticated();
            }, (err: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(err);
            });
    }

    routeToPreviousPageIfAuthenticated(): void {
        if (this.userService.isAuthenticated()) {
            const state: any = this.location.getState();
            let url: string = '/';

            if (state && state.requestedUrl) {
                url = state.requestedUrl;
            }

            this.router.navigate([url]);
        }
    }

    validateForm(): boolean {
        let valid: boolean = true;

        if (!this.user.email || !this.user.password) {
            valid = false;
        }

        this.formElement.nativeElement.classList.add('was-validated');

        return valid;
    }
}
