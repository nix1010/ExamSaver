import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Subject } from "../models/subject.model";

@Injectable()
export class SubjectService {

    constructor(private httpClient: HttpClient) { }

    public getTeachingSubjects(): Observable<Subject[]> {
        return this.httpClient.get<Subject[]>('subjects');
    }
}