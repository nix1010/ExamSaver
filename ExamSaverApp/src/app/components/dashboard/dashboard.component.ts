import { Role } from './../../models/role.model';
import { UserService } from './../../services/user.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Route } from '@angular/compiler/src/core';
import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

    Role = Role;

    constructor(
        private userService: UserService,
        private router: Router,
        private activatedRoute: ActivatedRoute
    ) { }

    ngOnInit(): void {
        if (this.router.url === '/exams') {
            if (this.userService.hasRoles([Role.PROFESSOR])) {
                this.router.navigate(['holding'], { relativeTo: this.activatedRoute });
            }
            else if (this.userService.hasRoles([Role.STUDENT])) {
                this.router.navigate(['taking'], { relativeTo: this.activatedRoute });
            }
        }
    }

}
