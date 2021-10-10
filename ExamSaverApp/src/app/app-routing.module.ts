
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErrorPageComponent } from './components/error-pages/error-page/error-page.component';
import { CreateExamComponent } from './components/exam/create-exam/create-exam.component';
import { UpdateExamComponent } from './components/exam/update-exam/update-exam.component';
import { LoginComponent } from './components/login/login.component';
import { Role } from './models/role.model';
import { RoleGuardService } from './services/role-guard.service';


const routes: Routes = [
    {
        path: '',
        redirectTo: 'exams',
        pathMatch: 'full'
    },
    {
        path: 'exams',
        children: [
            {
                path: 'create',
                component: CreateExamComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            },
            {
                path: 'update',
                component: UpdateExamComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            }
        ]
    },
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: '**',
        component: ErrorPageComponent
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {

}