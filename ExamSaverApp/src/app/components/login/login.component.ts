import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { USER_AUTHENTICATION_TOKEN_KEY } from 'src/app/config/constants';
import { AuthenticationResponse } from 'src/app/models/authentication-response';
import { User } from 'src/app/models/user-credentials.model';
import { getErrorResponseMessage } from 'src/app/utils/utils';
import { UserService } from './../../services/user.service';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
    public email: string = "milos@gmail.com";
    public password: string = "pass1";
    public errorMessage: string = null;
    public loginProcess: boolean = false;

    constructor(private userService: UserService, private router: Router) { }

    ngOnInit(): void {
        this.routeToMainPageIfAuthenticated();
    }

    authenticate() {
        this.loginProcess = true;
        this.userService.authenticate(new User(this.email, this.password))
            .pipe(finalize(() => this.loginProcess = false))
            .subscribe(() => {
                this.routeToMainPageIfAuthenticated();
            }, (err: HttpErrorResponse) => {
                this.errorMessage = getErrorResponseMessage(err);
            });
    }

    routeToMainPageIfAuthenticated(): void {
        if (this.userService.isAuthenticated()) {
            this.router.navigate(['/']);
        }
    }
}
