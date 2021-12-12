import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SimilarityCheckComponent } from './similarity-check.component';

describe('SimilarityCheckComponent', () => {
  let component: SimilarityCheckComponent;
  let fixture: ComponentFixture<SimilarityCheckComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SimilarityCheckComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SimilarityCheckComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
