import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators/map";

import { AuthenticationResponse } from 'src/app/models/authentication-response';
import { Role } from "../models/role.model";
import { User } from '../models/user-credentials.model';
import { USER_AUTHENTICATION_TOKEN_KEY } from '../config/constants';
import { DecodedToken } from './../interfaces/decoded-token.interface';

@Injectable()
export class UserService {
    private jwtHelperService = new JwtHelperService();
    private decodedToken: DecodedToken = null;

    constructor(private httpClient: HttpClient) {
        this.decodedToken = this.getAuthenticatedUser();
    }

    authenticate(user: User): Observable<AuthenticationResponse> {
        return this.httpClient.post<AuthenticationResponse>('users/authenticate', user)
            .pipe(map((observer: AuthenticationResponse) => {
                localStorage.setItem(USER_AUTHENTICATION_TOKEN_KEY, observer.token);
                this.decodedToken = this.getAuthenticatedUser();
                return observer;
            }));
    }

    logout(): void {
        localStorage.removeItem(USER_AUTHENTICATION_TOKEN_KEY);
        this.decodedToken = null;
    }

    isAuthenticated(): boolean {
        let token: string = localStorage.getItem(USER_AUTHENTICATION_TOKEN_KEY);
        if (token) {
            return !this.jwtHelperService.isTokenExpired(token);
        }
        return false;
    }

    getAuthenticatedUser(): DecodedToken {
        let decodedToken: DecodedToken = this.decodedToken;

        if (decodedToken === null) {
            let token: string = localStorage.getItem(USER_AUTHENTICATION_TOKEN_KEY);
            if (token) {
                decodedToken = this.jwtHelperService.decodeToken(token);
            }
        }

        return decodedToken;
    }

    hasRoles(roles: Role[]): boolean {
        if (this.decodedToken) {
            for (let role of roles) {
                let index = this.decodedToken.role.indexOf(role);
                if (index !== -1) {
                    return true;
                }
            }
        }
        
        return false;
    }
}