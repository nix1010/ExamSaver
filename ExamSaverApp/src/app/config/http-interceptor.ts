import { HOST, PORT } from './constants';
import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { USER_AUTHENTICATION_TOKEN_KEY } from "./constants";

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let headers: HttpHeaders = request.headers;
        headers = headers.set('Authorization', `Bearer ${localStorage.getItem(USER_AUTHENTICATION_TOKEN_KEY)}`);

        let updatedRequest = request.clone({
            url: `http://${HOST}:${PORT}/api/${request.url}`,
            headers: headers
        });

        return next.handle(updatedRequest);
    }

}