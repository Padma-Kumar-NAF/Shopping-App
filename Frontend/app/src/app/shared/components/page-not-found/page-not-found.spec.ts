import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { PageNotFound } from './page-not-found';

describe('PageNotFound', () => {
  let component: PageNotFound;
  let fixture: ComponentFixture<PageNotFound>;
  const navigate = vi.fn();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageNotFound],
      providers: [{ provide: Router, useValue: { navigate } }],
    }).compileComponents();

    fixture = TestBed.createComponent(PageNotFound);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('goHome should navigate to /', () => {
    component.goHome();
    expect(navigate).toHaveBeenCalledWith(['/']);
  });
});
