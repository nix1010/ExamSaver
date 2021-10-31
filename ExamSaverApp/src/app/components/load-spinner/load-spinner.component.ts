import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'app-load-spinner',
    templateUrl: './load-spinner.component.html',
    styleUrls: ['./load-spinner.component.scss']
})
export class LoadSpinnerComponent implements OnInit {

    @Input()
    size: string;

    constructor() { }

    ngOnInit(): void {
    }

}
