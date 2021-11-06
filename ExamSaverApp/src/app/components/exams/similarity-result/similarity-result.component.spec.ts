import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SimilarityResultComponent } from './similarity-result.component';

describe('SimilarityResultComponent', () => {
  let component: SimilarityResultComponent;
  let fixture: ComponentFixture<SimilarityResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SimilarityResultComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SimilarityResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
