import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable } from "rxjs";
import { map } from "rxjs/internal/operators/map";
import { AuthenticationResponse } from 'src/app/models/authentication-response.model';
import { USER_AUTHENTICATION_TOKEN_KEY as AUTHENTICATED_USER_KEY } from '../config/constants';
import { AuthenticatedUser } from "../models/authenticated-user.model";
import { Role } from "../models/role.model";
import { User } from '../models/user-credentials.model';
import { DecodedToken } from './../interfaces/decoded-token.interface';


@Injectable()
export class UserService {
    private jwtHelperService = new JwtHelperService();
    private authenticatedUser: AuthenticatedUser;
    private decodedToken: DecodedToken;

    constructor(private httpClient: HttpClient) {
        this.setAuthenticatedUserFromStorage();
    }

    authenticate(user: User): Observable<AuthenticationResponse> {
        return this.httpClient.post<AuthenticationResponse>('users/authenticate', user)
            .pipe(map((authenticationResponse: AuthenticationResponse) => {
                this.setAuthenticatedUser(authenticationResponse);
                return authenticationResponse;
            }));
    }

    logout(): void {
        localStorage.removeItem(AUTHENTICATED_USER_KEY);
        this.authenticatedUser = null;
        this.decodedToken = null;
    }

    isAuthenticated(): boolean {
        if (this.authenticatedUser) {
            return !this.jwtHelperService.isTokenExpired(this.authenticatedUser.token);
        }

        return false;
    }

    private setAuthenticatedUser(authenticationResponse: AuthenticationResponse): void {
        this.authenticatedUser = new AuthenticatedUser();
        this.authenticatedUser.firstName = authenticationResponse.firstName;
        this.authenticatedUser.lastName = authenticationResponse.lastName;
        this.authenticatedUser.token = authenticationResponse.jwtToken.token;

        this.setDecodedToken();

        localStorage.setItem(AUTHENTICATED_USER_KEY, JSON.stringify(this.authenticatedUser));
    }

    private setAuthenticatedUserFromStorage(): void {
        let authenticatedUserJSON = localStorage.getItem(AUTHENTICATED_USER_KEY);

        try {
            this.authenticatedUser = JSON.parse(authenticatedUserJSON);
            this.setDecodedToken();
        }
        catch (error) { }
    }

    getAuthenticatedUser(): AuthenticatedUser {
        return this.authenticatedUser;
    }

    private setDecodedToken(): void {
        if (this.authenticatedUser) {
            this.decodedToken = this.jwtHelperService.decodeToken(this.authenticatedUser.token);
        }
    }

    hasRoles(roles: Role[]): boolean {
        if (this.decodedToken) {
            for (let role of roles) {
                let index = this.decodedToken.role.indexOf(role);
                if (index === -1) {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}