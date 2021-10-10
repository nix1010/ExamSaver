import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from "@angular/router";
import { Observable } from "rxjs";
import { AuthGuardService } from './auth-guard.service';
import { UserService } from "./user.service";

@Injectable()
export class RoleGuardService implements CanActivate {

    constructor(private userService: UserService,
        private authGuardService: AuthGuardService) { }

    canActivate(route: ActivatedRouteSnapshot, _state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
        let canActivate: boolean = this.authGuardService.canActivate(route, _state);

        if (canActivate && this.userService.hasRoles(route.data.roles)) {
            return true;
        }

        return false;
    }

}