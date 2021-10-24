import { HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Exam } from 'src/app/models/exam.model';
import { Page } from 'src/app/models/page.model';
import { Role } from 'src/app/models/role.model';
import { getErrorResponseMessage, getExamsUri, unsubscribeFrom } from 'src/app/utils/utils';
import { ExamService } from './../../services/exam.service';

@Component({
    selector: 'app-exams',
    templateUrl: './exams.component.html',
    styleUrls: ['./exams.component.scss']
})
export class ExamsComponent implements OnInit, OnDestroy {
    public exams: Exam[] = [];
    public role: Role = null;
    public page: Page = new Page(1, 0, 0);
    
    public showSpinner: boolean = false;
    public showContent: boolean = false;
    public errorMessage: string = null;

    routerEventsSubscription: Subscription;
    examsSubscription: Subscription;

    Role = Role;

    constructor(
        private examService: ExamService,
        private router: Router,
        private activatedRoute: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.role = this.activatedRoute.snapshot.data.roles[0];
        this.setPageFromQueryParams();
        this.getExams();

        this.routerEventsSubscription = this.router.events.subscribe((event: NavigationEnd) => {
            if (event instanceof NavigationEnd) {
                this.getExams();
            }
        });
    }

    ngOnDestroy(): void {
        unsubscribeFrom(this.routerEventsSubscription);
        unsubscribeFrom(this.examsSubscription);
    }

    getTitle(): string {
        if (this.role === Role.PROFESSOR) {
            return 'Holding exams';
        }

        return 'Active exams';
    }

    getExams(): void {
        this.showSpinner = true;
        this.errorMessage = null;
        this.showContent = false;

        unsubscribeFrom(this.examsSubscription);

        this.examsSubscription = this.getExamsByRole()
            .pipe(finalize(() => this.showSpinner = false))
            .subscribe((response: HttpResponse<Exam[]>) => {
                this.page = this.getPageHeader(response.headers);
                this.exams = response.body
                this.showContent = true;
            },
                (error: HttpErrorResponse) => this.errorMessage = getErrorResponseMessage(error));
    }

    getExamsByRole(): Observable<HttpResponse<Exam[]>> {
        if (this.role === Role.PROFESSOR) {
            return this.examService.getHoldingExams(this.page.currentPage);
        }

        return this.examService.getTakingExams(this.page.currentPage);
    }

    setPageFromQueryParams(): void {
        let pageParam: string = this.activatedRoute.snapshot.queryParamMap.get('page');

        if (pageParam) {
            let page: number = Number(pageParam);

            if (!Number.isNaN(page)) {
                this.page.currentPage = page;
            }
        }
    }

    getPageHeader(httpHeaders: HttpHeaders): Page {
        let xPagination: string = httpHeaders.get('X-Pagination');

        let page: Page = this.page;

        if (xPagination) {
            page = JSON.parse(xPagination);
        }

        return page;
    }

    pageChange(): void {
        this.router.navigate([getExamsUri(this.role)], { queryParams: { page: this.page.currentPage } });
    }
}
