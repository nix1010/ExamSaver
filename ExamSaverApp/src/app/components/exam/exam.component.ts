import { UserService } from './../../services/user.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { Role } from 'src/app/models/role.model';

@Component({
    selector: 'app-exams',
    templateUrl: './exam.component.html',
    styleUrls: ['./exam.component.scss']
})
export class ExamComponent implements OnInit {

    Role = Role;

    constructor(
        private userService: UserService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        if (this.router.url === '/exams') {
            if (this.userService.hasRoles([Role.PROFESSOR])) {
                this.router.navigate(['holding'], { relativeTo: this.route });
            }
            else if (this.userService.hasRoles([Role.STUDENT])) {
                this.router.navigate(['taking'], { relativeTo: this.route });
            }
        }
    }
}
