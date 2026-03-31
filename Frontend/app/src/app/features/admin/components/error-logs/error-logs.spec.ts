import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { ErrorLogs } from './error-logs';
import { ErrorLogsService } from '../../../services/adminServices/error-logs.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ErrorLogs', () => {
  let component: ErrorLogs;
  let fixture: ComponentFixture<ErrorLogs>;

  const getErrorLogs = vi.fn();

  const mockLogs = [
    { id: '1', statusCode: 500, message: 'Internal Server Error', timestamp: '2026-01-01T10:00:00' },
    { id: '2', statusCode: 404, message: 'Not Found', timestamp: '2026-01-01T09:00:00' },
    { id: '3', statusCode: 200, message: 'OK', timestamp: '2026-01-01T08:00:00' },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    getErrorLogs.mockReturnValue(of({ data: { items: mockLogs, totalCount: 3 } }));

    await TestBed.configureTestingModule({
      imports: [ErrorLogs],
      providers: [{ provide: ErrorLogsService, useValue: { getErrorLogs } }],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ErrorLogs);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load logs on init', () => {
    expect(getErrorLogs).toHaveBeenCalled();
    expect(component.logs().length).toBe(3);
    expect(component.isLoading()).toBe(false);
  });

  it('should set totalItems and totalPages after loading', () => {
    expect(component.totalItems()).toBe(3);
    expect(component.totalPages()).toBeGreaterThan(0);
  });

  it('should set isLoading to false on load failure', () => {
    getErrorLogs.mockReturnValue(throwError(() => ({ error: { message: 'Error' } })));
    component.loadLogs();
    expect(component.isLoading()).toBe(false);
  });

  describe('onPageChange', () => {
    it('should update currentPage and reload logs', () => {
      component.onPageChange(2);
      expect(component.currentPage()).toBe(2);
      expect(getErrorLogs).toHaveBeenCalledTimes(2);
    });
  });

  describe('toggleSort', () => {
    it('should toggle sortAsc and reverse logs', () => {
      const originalFirst = component.logs()[0];
      component.toggleSort();
      expect(component.sortAsc()).toBe(true);
      expect(component.logs()[0]).not.toEqual(originalFirst);
    });

    it('should toggle back to descending', () => {
      component.toggleSort();
      component.toggleSort();
      expect(component.sortAsc()).toBe(false);
    });
  });

  describe('statusRowClass', () => {
    it('should return red class for 5xx errors', () => {
      expect(component.statusRowClass(500)).toContain('red');
    });

    it('should return yellow class for 4xx errors', () => {
      expect(component.statusRowClass(404)).toContain('yellow');
    });

    it('should return default class for 2xx', () => {
      expect(component.statusRowClass(200)).not.toContain('red');
    });
  });

  describe('statusBadgeClass', () => {
    it('should return red badge for 5xx', () => {
      expect(component.statusBadgeClass(500)).toContain('red');
    });

    it('should return yellow badge for 4xx', () => {
      expect(component.statusBadgeClass(400)).toContain('yellow');
    });

    it('should return green badge for 2xx', () => {
      expect(component.statusBadgeClass(200)).toContain('green');
    });

    it('should return gray badge for other codes', () => {
      expect(component.statusBadgeClass(100)).toContain('gray');
    });
  });
});
