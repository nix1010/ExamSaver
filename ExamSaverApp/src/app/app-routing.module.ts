import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ErrorPageComponent } from './components/error-pages/error-page/error-page.component';
import { AddUpdateExamComponent } from './components/exams/add-update-exam/add-update-exam.component';
import { ExamsComponent } from './components/exams/exams.component';
import { FileUploadComponent } from './components/exams/file-upload/file-upload.component';
import { FileExplorerComponent } from './components/exams/student-exams/student-exam/file-explorer/file-explorer.component';
import { FileViewerComponent } from './components/exams/student-exams/student-exam/file-viewer/file-viewer.component';
import { StudentExamComponent } from './components/exams/student-exams/student-exam/student-exam.component';
import { StudentExamsComponent } from './components/exams/student-exams/student-exams.component';
import { LoginComponent } from './components/login/login.component';
import { Role } from './models/role.model';
import { AuthGuardService } from './services/auth-guard.service';
import { RoleGuardService } from './services/role-guard.service';


const routes: Routes = [
    {
        path: '',
        redirectTo: 'exams',
        pathMatch: 'full'
    },
    {
        path: 'exams',
        component: DashboardComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: 'taking',
                component: ExamsComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.STUDENT]
                }
            },
            {
                path: 'taking/:examId/submit',
                component: FileUploadComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.STUDENT]
                }
            },
            {
                path: 'holding',
                component: ExamsComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            },
            {
                path: 'holding/add',
                component: AddUpdateExamComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            },
            {
                path: 'holding/:examId/update',
                component: AddUpdateExamComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            },
            {
                path: 'holding/:examId/students',
                component: StudentExamsComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                }
            },
            {
                path: 'holding/:examId/students/:studentId',
                component: StudentExamComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                },
                children: [
                    {
                        path: '',
                        pathMatch: 'full',
                        redirectTo: 'tree'
                    },
                    {
                        path: 'tree',
                        component: FileExplorerComponent,
                        canActivate: [RoleGuardService],
                        data: {
                            roles: [Role.PROFESSOR]
                        },
                        children: [
                            {
                                path: '**',
                                component: FileExplorerComponent,
                                canActivate: [RoleGuardService],
                                data: {
                                    roles: [Role.PROFESSOR]
                                }
                            }
                        ]
                    },
                    {
                        path: 'file',
                        component: FileViewerComponent,
                        canActivate: [RoleGuardService],
                        data: {
                            roles: [Role.PROFESSOR]
                        },
                        children: [
                            {
                                path: '**',
                                component: FileViewerComponent,
                                canActivate: [RoleGuardService],
                                data: {
                                    roles: [Role.PROFESSOR]
                                }
                            }
                        ]
                    }
                ]
            },
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