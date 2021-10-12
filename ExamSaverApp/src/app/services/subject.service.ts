import { HttpClient } from '@angular/common/http';
import { Injectable } from "@angular/core";
import { Subject } from "../models/subject.model";

@Injectable()
export class SubjectService {

    private subjects: Subject[] = [];
    private subjectsObtained: boolean = false;

    constructor(private httpClient: HttpClient) { }

    getTeachingSubjects(): Subject[] {
        if (!this.subjectsObtained) {
            this.obtainTeachingSubjects();
        }

        return this.subjects;
    }

    private obtainTeachingSubjects(): void {
        this.httpClient.get('subjects')
            .subscribe((subjectsResponse: Subject[]) => {
                this.subjects = subjectsResponse;
                this.subjectsObtained = true;
            });
    }

}