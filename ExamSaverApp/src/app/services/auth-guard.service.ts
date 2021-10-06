import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { Observable } from "rxjs";
import { LOGIN_ABSOLUTE_ROUTE } from "../config/constants";
import { UserService } from './user.service';

@Injectable()
export class AuthGuardService implements CanActivate {

    constructor(private userService: UserService, private router: Router) { }

    canActivate(): boolean {
        if (this.userService.isAuthenticated()) {
            return true;
        }
        this.router.navigate([LOGIN_ABSOLUTE_ROUTE]);
        
        return false;
    }

}