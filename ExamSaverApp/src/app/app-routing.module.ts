import { StudentExamComponent } from './components/exams/student-exam/student-exam.component';
import { AuthGuardService } from './services/auth-guard.service';
import { StudentListComponent } from './components/exams/student-list/student-list.component';
import { ExamListComponent } from './components/exams/exam-list/exam-list.component';
import { FileViewerComponent } from './components/exams/student-exam/file-viewer/file-viewer.component';
import { FileUploadComponent } from './components/exams/file-upload/file-upload.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErrorPageComponent } from './components/error-pages/error-page/error-page.component';
import { AddUpdateExamComponent as AddUpdateExamComponent } from './components/exams/add-update-exam/add-update-exam.component';
import { ExamsComponent } from './components/exams/exams.component';
import { LoginComponent } from './components/login/login.component';
import { Role } from './models/role.model';
import { RoleGuardService } from './services/role-guard.service';
import { FileExplorerComponent } from './components/exams/student-exam/file-explorer/file-explorer.component';


const routes: Routes = [
    {
        path: '',
        redirectTo: 'exams',
        pathMatch: 'full'
    },
    {
        path: 'exams',
        component: ExamsComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: 'taking',
                component: ExamListComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.STUDENT]
                },
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
                component: ExamListComponent,
                canActivate: [RoleGuardService],
                data: {
                    roles: [Role.PROFESSOR]
                },
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
                component: StudentListComponent,
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