import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LOGIN_ABSOLUTE_ROUTE } from 'src/app/config/constants';
import { Role } from './../../models/role.model';
import { UserService } from './../../services/user.service';

@Component({
    selector: 'app-nav-bar',
    templateUrl: './nav-bar.component.html',
    styleUrls: ['./nav-bar.component.scss']
})
export class NavBarComponent implements OnInit {

    public Role = Role;

    constructor(
        public userService: UserService,
        private router: Router
    ) { }

    ngOnInit(): void {
    }

    getFirstName(): string {
        return this.userService.getAuthenticatedUser()?.firstName;
    }

    getLastName(): string {
        return this.userService.getAuthenticatedUser()?.lastName;
    }

    logout(): void {
        this.userService.logout();
        this.router.navigate([LOGIN_ABSOLUTE_ROUTE]);
    }

}
