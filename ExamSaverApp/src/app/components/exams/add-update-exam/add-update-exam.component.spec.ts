import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddUpdateExamComponent } from './add-update-exam.component';

describe('AddUpdateExamComponent', () => {
  let component: AddUpdateExamComponent;
  let fixture: ComponentFixture<AddUpdateExamComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddUpdateExamComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddUpdateExamComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
