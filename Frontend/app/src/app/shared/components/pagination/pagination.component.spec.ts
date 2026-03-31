import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';
import { CommonModule } from '@angular/common';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent, CommonModule],
    }).compileComponents();

    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Inputs', () => {
    it('should have default input values', () => {
      expect(component.currentPage).toBe(1);
      expect(component.totalPages).toBe(1);
      expect(component.totalItems).toBe(0);
      expect(component.pageSize).toBe(10);
      expect(component.isLoading).toBe(false);
      expect(component.maxVisiblePages).toBe(5);
    });

    it('should accept custom input values', () => {
      component.currentPage = 3;
      component.totalPages = 10;
      expect(component.currentPage).toBe(3);
      expect(component.totalPages).toBe(10);
    });
  });

  describe('Outputs', () => {
    it('should emit pageChange when goToPage is called with valid page', () => {
      component.currentPage = 1;
      component.totalPages = 5;
      component.isLoading = false;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.goToPage(3);
      expect(emitted).toContain(3);
    });

    it('should have pageSizeChange output defined', () => {
      expect(component.pageSizeChange).toBeDefined();
    });
  });

  describe('Computed getters', () => {
    beforeEach(() => {
      component.currentPage = 2;
      component.totalPages = 5;
      component.totalItems = 50;
      component.pageSize = 10;
      component.isLoading = false;
    });

    it('should compute startItem correctly', () => {
      expect(component.startItem).toBe(11);
    });

    it('should compute endItem correctly', () => {
      expect(component.endItem).toBe(20);
    });

    it('should compute canGoPrevious correctly', () => {
      expect(component.canGoPrevious).toBe(true);
      component.currentPage = 1;
      expect(component.canGoPrevious).toBe(false);
    });

    it('should return false for canGoPrevious when loading', () => {
      component.isLoading = true;
      expect(component.canGoPrevious).toBe(false);
    });

    it('should compute canGoNext correctly', () => {
      expect(component.canGoNext).toBe(true);
      component.currentPage = 5;
      expect(component.canGoNext).toBe(false);
    });

    it('should return false for canGoNext when loading', () => {
      component.isLoading = true;
      expect(component.canGoNext).toBe(false);
    });
  });

  describe('Navigation methods', () => {
    beforeEach(() => {
      component.currentPage = 3;
      component.totalPages = 5;
      component.isLoading = false;
    });

    it('previousPage should emit currentPage - 1', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.previousPage();
      expect(emitted).toContain(2);
    });

    it('previousPage should not emit when on first page', () => {
      component.currentPage = 1;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.previousPage();
      expect(emitted.length).toBe(0);
    });

    it('nextPage should emit currentPage + 1', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.nextPage();
      expect(emitted).toContain(4);
    });

    it('nextPage should not emit when on last page', () => {
      component.currentPage = 5;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.nextPage();
      expect(emitted.length).toBe(0);
    });

    it('firstPage should emit 1', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.firstPage();
      expect(emitted).toContain(1);
    });

    it('firstPage should not emit when already on first page', () => {
      component.currentPage = 1;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.firstPage();
      expect(emitted.length).toBe(0);
    });

    it('lastPage should emit totalPages', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.lastPage();
      expect(emitted).toContain(5);
    });

    it('lastPage should not emit when already on last page', () => {
      component.currentPage = 5;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.lastPage();
      expect(emitted.length).toBe(0);
    });

    it('goToPage should not emit for out-of-range page', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.goToPage(0);
      component.goToPage(6);
      expect(emitted.length).toBe(0);
    });

    it('goToPage should not emit for current page', () => {
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.goToPage(3);
      expect(emitted.length).toBe(0);
    });

    it('goToPage should not emit when loading', () => {
      component.isLoading = true;
      const emitted: number[] = [];
      component.pageChange.subscribe((p: number) => emitted.push(p));
      component.goToPage(2);
      expect(emitted.length).toBe(0);
    });
  });
});
