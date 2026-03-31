import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { UnauthorizedComponent } from './unauthorized';
import { AuthStateService } from '../../services/auth-state.service';

describe('UnauthorizedComponent', () => {
  let component: UnauthorizedComponent;
  let fixture: ComponentFixture<UnauthorizedComponent>;
  const navigate = vi.fn();
  const role = vi.fn();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UnauthorizedComponent],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: AuthStateService, useValue: { role } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UnauthorizedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('goToHome should navigate to /admin when role is admin', () => {
    role.mockReturnValue('admin');
    component.goToHome();
    expect(navigate).toHaveBeenCalledWith(['/admin']);
  });

  it('goToHome should navigate to / when role is not admin', () => {
    role.mockReturnValue('user');
    component.goToHome();
    expect(navigate).toHaveBeenCalledWith(['/']);
  });
});
