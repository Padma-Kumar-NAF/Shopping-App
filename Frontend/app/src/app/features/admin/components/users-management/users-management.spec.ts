import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { UsersManagement } from './users-management';
import { UserServcie } from '../../../services/adminServices/user.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { AuthStateService } from '../../../services/auth-state.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('UsersManagement', () => {
  let component: UsersManagement;
  let fixture: ComponentFixture<UsersManagement>;

  const getAllUser = vi.fn();
  const changeUserRole = vi.fn();
  const activateUser = vi.fn();
  const deactivateUser = vi.fn();
  const setUsers = vi.fn();
  const updateUser = vi.fn();
  const email = vi.fn().mockReturnValue('current@example.com');

  const mockUsers: any[] = [
    { userId: 'u1', name: 'Alice', email: 'alice@example.com', role: 'user', activeStatus: true },
    { userId: 'u2', name: 'Bob', email: 'bob@example.com', role: 'admin', activeStatus: false },
  ];

  const storeState$ = new BehaviorSubject({ users: mockUsers });

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllUser.mockReturnValue(of({ data: { usersList: mockUsers } }));
    email.mockReturnValue('current@example.com');

    await TestBed.configureTestingModule({
      imports: [UsersManagement],
      providers: [
        { provide: UserServcie, useValue: { getAllUser, changeUserRole, activateUser, deactivateUser } },
        {
          provide: StoreService,
          useValue: {
            state$: storeState$.asObservable(),
            value: { users: mockUsers },
            pageCache: { users: new Set([1]) },
            setUsers, updateUser,
          },
        },
        { provide: AuthStateService, useValue: { email } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(UsersManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('pagedUsers', () => {
    it('should return filtered users excluding current user', () => {
      const paged = component.pagedUsers;
      expect(paged.every((u: any) => u.email !== 'current@example.com')).toBe(true);
    });
  });

  describe('updateSearch', () => {
    it('should update searchTerm and reset page', () => {
      component.currentPage.set(3);
      const event = { target: { value: 'alice' } } as any;
      component.updateSearch(event);
      expect(component.searchTerm()).toBe('alice');
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('updateRoleFilter', () => {
    it('should update filterRole and reset page', () => {
      component.currentPage.set(2);
      const event = { target: { value: 'admin' } } as any;
      component.updateRoleFilter(event);
      expect(component.filterRole()).toBe('admin');
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('changeUserRole', () => {
    it('should call changeUserRole API and update store on success', () => {
      changeUserRole.mockReturnValue(of({ data: { isChanged: true }, message: 'Updated' }));
      component.changeUserRole('u1', 'admin');
      expect(changeUserRole).toHaveBeenCalledWith('u1', 'admin');
      expect(updateUser).toHaveBeenCalled();
    });
  });

  describe('Status toggle modal', () => {
    it('openStatusModal should set pending toggle and show modal', () => {
      component.openStatusModal('u1', true);
      expect(component.pendingToggleId()).toBe('u1');
      expect(component.pendingToggleAction()).toBe('deactivate');
      expect(component.confirmModal()).toBe(true);
    });

    it('openStatusModal should set activate action for inactive user', () => {
      component.openStatusModal('u2', false);
      expect(component.pendingToggleAction()).toBe('activate');
    });

    it('cancelStatusToggle should hide modal and clear pending id', () => {
      component.openStatusModal('u1', true);
      component.cancelStatusToggle();
      expect(component.confirmModal()).toBe(false);
      expect(component.pendingToggleId()).toBeNull();
    });

    it('confirmStatusToggle should call deactivateUser and update store', () => {
      deactivateUser.mockReturnValue(of({}));
      component.pendingToggleId.set('u1');
      component.pendingToggleAction.set('deactivate');
      component.confirmStatusToggle();
      expect(deactivateUser).toHaveBeenCalledWith('u1');
      expect(updateUser).toHaveBeenCalled();
    });

    it('confirmStatusToggle should call activateUser for activate action', () => {
      activateUser.mockReturnValue(of({}));
      component.pendingToggleId.set('u2');
      component.pendingToggleAction.set('activate');
      component.confirmStatusToggle();
      expect(activateUser).toHaveBeenCalledWith('u2');
    });
  });

  describe('Pagination', () => {
    it('prevPage should decrement currentPage', () => {
      component.currentPage.set(2);
      component.prevPage();
      expect(component.currentPage()).toBe(1);
    });

    it('prevPage should not go below 1', () => {
      component.currentPage.set(1);
      component.prevPage();
      expect(component.currentPage()).toBe(1);
    });

    it('goToPage should set currentPage when data exists', () => {
      component.goToPage(1);
      expect(component.currentPage()).toBe(1);
    });
  });

  describe('getRoleColor', () => {
    it('should return purple for admin', () => {
      expect(component.getRoleColor('admin')).toContain('purple');
    });

    it('should return blue for user', () => {
      expect(component.getRoleColor('user')).toContain('blue');
    });
  });
});
