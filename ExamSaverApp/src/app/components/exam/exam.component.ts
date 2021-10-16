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
        private router: Router
    ) { }

    ngOnInit(): void {
        let holdingExamsUri: string = '/exams/holding';
        let takingExamsUri: string = '/exams/taking';

        if (this.router.url !== holdingExamsUri && this.router.url !== takingExamsUri) {
            if (this.userService.hasRoles([Role.PROFESSOR])) {
                this.router.navigate([holdingExamsUri]);
            }
            else {
                this.router.navigate([takingExamsUri]);
            }
        }
    }
}
