import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthenticatedUser } from './../models/authenticated-user.model';
import { UserService } from './../services/user.service';
import { HOST, PORT } from './constants';

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {

    constructor(private userService: UserService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let authenticatedUser: AuthenticatedUser = this.userService.getAuthenticatedUser();
        let headers: HttpHeaders = request.headers;
        
        headers = headers.set('Authorization', `Bearer ${authenticatedUser?.token}`);

        const updatedRequest = request.clone({
            url: `http://${HOST}:${PORT}/api/${request.url}`,
            headers: headers
        });

        return next.handle(updatedRequest);
    }

}