import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { LOGIN_ABSOLUTE_ROUTE } from "../config/constants";
import { UserService } from './user.service';

@Injectable()
export class AuthGuardService implements CanActivate {

    constructor(
        private userService: UserService,
        private router: Router
    ) { }

    canActivate(_routeSnapshot: ActivatedRouteSnapshot, stateSnapshot: RouterStateSnapshot): boolean {
        if (this.userService.isAuthenticated()) {
            return true;
        }

        this.router.navigate([LOGIN_ABSOLUTE_ROUTE], { state: { requestedUrl: stateSnapshot.url } });

        return false;
    }

}