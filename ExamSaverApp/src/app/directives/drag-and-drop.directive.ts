import {
    Directive,
    EventEmitter,
    HostBinding,
    HostListener,
    Output
} from '@angular/core';

@Directive({
    selector: '[DragAndDrop]'
})
export class DragAndDropDirective {
    @HostBinding('class.file-over')
    private fileOver: boolean = false;

    @Output()
    private fileDropped: EventEmitter<FileList> = new EventEmitter<FileList>();

    constructor() { }

    @HostListener('dragover', ['$event']) onDragOver(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();

        this.fileOver = true;
    }

    @HostListener('dragleave', ['$event']) public onDragLeave(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = false;
    }

    @HostListener('drop', ['$event']) public ondrop(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();

        this.fileOver = false;

        let files: FileList = event.dataTransfer.files;
        if (files.length > 0) {
            this.fileDropped.emit(files);
        }
    }

}
