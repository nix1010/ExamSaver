import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { Observable } from "rxjs";
import { LOGIN_ABSOLUTE_ROUTE } from '../config/constants';
import { UserService } from "./user.service";

@Injectable()
export class RoleGuardService implements CanActivate {

    constructor(private userService: UserService,
        private router: Router) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
        if (this.userService.hasRoles(route.data.allowedRoles)) {
            return true;
        }
        this.router.navigate([LOGIN_ABSOLUTE_ROUTE]);
        
        return false;
    }

}