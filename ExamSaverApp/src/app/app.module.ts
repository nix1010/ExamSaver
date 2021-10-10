import { UserService } from './services/user.service';
import { AuthGuardService } from './services/auth-guard.service';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ErrorPageComponent } from './components/error-pages/error-page/error-page.component';
import { LoginComponent } from './components/login/login.component';
import { NavBarComponent } from './components/nav-bar/nav-bar.component';
import { HttpRequestInterceptor } from './config/http-interceptor';
import { RoleGuardService } from './services/role-guard.service';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { DragAndDropDirective } from './directives/drag-and-drop.directive';
import { CreateExamComponent } from './components/exam/create-exam/create-exam.component';
import { ExamsComponent } from './components/exam/exams/exams.component';
import { UpdateExamComponent } from './components/exam/update-exam/update-exam.component';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    NavBarComponent,
    ErrorPageComponent,
    ExamsComponent,
    FileUploadComponent,
    DragAndDropDirective,
    CreateExamComponent,
    UpdateExamComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: HttpRequestInterceptor, multi: true },
    AuthGuardService,
    RoleGuardService,
    UserService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
