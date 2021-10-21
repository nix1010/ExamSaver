import { AfterViewInit, Directive, ElementRef, Input } from '@angular/core';
import { HighlightConfig } from '../interfaces/highlight-config.interface';

declare var hljs: any;

@Directive({
    selector: '[appHighlight]'
})
export class HighlightDirective implements AfterViewInit {
    @Input('appHighlight')
    private config: HighlightConfig;

    constructor(private elementRef: ElementRef) { }

    ngAfterViewInit(): void {
        this.setupHighlight(this.elementRef);
    }

    private setupHighlight(elementRef: ElementRef): void {
        hljs.highlightElement(elementRef.nativeElement);

        if (this.config) {
            if (this.config.lineNumbers) {
                hljs.lineNumbersBlock(elementRef.nativeElement);
            }
        }
    }
}
