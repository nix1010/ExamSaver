import { UserService } from './services/user.service';
import { AuthGuardService } from './services/auth-guard.service';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { PageNotFoundComponent } from './components/error-pages/page-not-found/page-not-found.component';
import { LoginComponent } from './components/login/login.component';
import { NavBarComponent } from './components/nav-bar/nav-bar.component';
import { HttpRequestInterceptor } from './config/http-interceptor';
import { RoleGuardService } from './services/role-guard.service';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { DragAndDropDirective } from './directives/drag-and-drop.directive';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    NavBarComponent,
    PageNotFoundComponent,
    DashboardComponent,
    FileUploadComponent,
    DragAndDropDirective
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
