import { Role } from './../../models/role.model';
import { Router } from '@angular/router';
import { UserService } from './../../services/user.service';
import { Component, OnInit } from '@angular/core';
import { LOGIN_ABSOLUTE_ROUTE } from 'src/app/config/constants';

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

    logout(): void {
        this.userService.logout();
        this.router.navigate([LOGIN_ABSOLUTE_ROUTE]);
    }

}
