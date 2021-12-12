import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { UserService } from './../services/user.service';

@Directive({
    selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {

    @Input('appHasRole')
    private roles: any;
    private visible: boolean = false;

    constructor(
        private userService: UserService,
        private viewContainerRef: ViewContainerRef,
        private templateRef: TemplateRef<any>
    ) { }

    ngOnInit() {
        if (Object.prototype.toString.call(this.roles) !== '[object Array]') {
            this.roles = [this.roles];
        }

        if (this.userService.hasRoles(this.roles)) {
            if (!this.visible) {
                this.visible = true;
                this.viewContainerRef.createEmbeddedView(this.templateRef);
            }
        } else {
            this.visible = false;
            this.viewContainerRef.clear();
        }
    }
}
