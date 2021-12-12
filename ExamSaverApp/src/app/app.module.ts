import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ErrorPageComponent } from './components/error-pages/error-page/error-page.component';
import { AddUpdateExamComponent } from './components/exams/add-update-exam/add-update-exam.component';
import { ExamListComponent } from './components/exams/exam-list/exam-list.component';
import { ExamsComponent } from './components/exams/exams.component';
import { FileUploadComponent } from './components/exams/file-upload/file-upload.component';
import { StudentExamsComponent } from './components/exams/student-exams/student-exams.component';
import { LoadSpinnerComponent } from './components/load-spinner/load-spinner.component';
import { LoginComponent } from './components/login/login.component';
import { NavBarComponent } from './components/nav-bar/nav-bar.component';
import { HttpRequestInterceptor } from './config/http-interceptor';
import { DragAndDropDirective } from './directives/drag-and-drop.directive';
import { HasRoleDirective } from './directives/has-role.directive';
import { AuthGuardService } from './services/auth-guard.service';
import { ExamService } from './services/exam.service';
import { RoleGuardService } from './services/role-guard.service';
import { SubjectService } from './services/subject.service';
import { UserService } from './services/user.service';
import { HighlightDirective } from './directives/highlight.directive';
import { NgbDropdownModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { FileExplorerComponent } from './components/exams/student-exams/student-exam/file-explorer/file-explorer.component';
import { FileViewerComponent } from './components/exams/student-exams/student-exam/file-viewer/file-viewer.component';
import { SimilarityCheckComponent } from './components/exams/student-exams/similarity-check/similarity-check.component';
import { SimilarityResultComponent } from './components/exams/student-exams/similarity-check/similarity-result/similarity-result.component';
import { StudentExamComponent } from './components/exams/student-exams/student-exam/student-exam.component';
import { ExamItemComponent } from './components/exams/exam-list/exam-item/exam-item.component';


@NgModule({
    declarations: [
        AppComponent,
        LoginComponent,
        ExamsComponent,
        FileUploadComponent,
        AddUpdateExamComponent,
        NavBarComponent,
        ErrorPageComponent,
        FileUploadComponent,
        DragAndDropDirective,
        ExamListComponent,
        FileExplorerComponent,
        FileViewerComponent,
        StudentExamsComponent,
        LoadSpinnerComponent,
        HasRoleDirective,
        StudentExamComponent,
        HighlightDirective,
        DashboardComponent,
        SimilarityResultComponent,
        ExamItemComponent,
        SimilarityCheckComponent,
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        HttpClientModule,
        NgbPaginationModule,
        NgbDropdownModule
    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: HttpRequestInterceptor,
            multi: true
        },
        AuthGuardService,
        RoleGuardService,
        UserService,
        SubjectService,
        ExamService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
