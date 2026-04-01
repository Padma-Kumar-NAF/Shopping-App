import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { PromocodeManagement } from './promocode-management';
import { PromoCodeService } from '../../../services/adminServices/promocode.service';
import { StoreService } from '../../../services/adminServices/store.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('PromocodeManagement', () => {
  let component: PromocodeManagement;
  let fixture: ComponentFixture<PromocodeManagement>;

  const getAllPromoCodes = vi.fn();
  const addPromoCode = vi.fn();
  const editPromoCode = vi.fn();
  const deletePromoCode = vi.fn();
  const appendPromoCodes = vi.fn();
  const addPromoCodeStore = vi.fn();
  const updatePromoCode = vi.fn();
  const removePromoCode = vi.fn();

  const futureDate = new Date();
  futureDate.setFullYear(futureDate.getFullYear() + 1);
  const futureDateStr = futureDate.toISOString().split('T')[0];

  const pastDate = new Date();
  pastDate.setFullYear(pastDate.getFullYear() - 1);
  const pastDateStr = pastDate.toISOString().split('T')[0];

  const mockPromos: any[] = [
    { promoCodeId: 'pc1', promoCodeName: 'SAVE10', discountPercentage: 10, fromDate: new Date().toISOString(), toDate: futureDate.toISOString() },
    { promoCodeId: 'pc2', promoCodeName: 'OLD20', discountPercentage: 20, fromDate: pastDate.toISOString(), toDate: pastDate.toISOString() },
  ];

  const storeState$ = new BehaviorSubject({ promoCodes: mockPromos });

  beforeEach(async () => {
    vi.clearAllMocks();
    getAllPromoCodes.mockReturnValue(of({ data: { promoCodes: mockPromos } }));

    await TestBed.configureTestingModule({
      imports: [PromocodeManagement],
      providers: [
        { provide: PromoCodeService, useValue: { getAllPromoCodes, addPromoCode, editPromoCode, deletePromoCode } },
        {
          provide: StoreService,
          useValue: {
            state$: storeState$.asObservable(),
            value: { promoCodes: mockPromos },
            pageCache: { promoCodes: new Set([1]) },
            appendPromoCodes, addPromoCode: addPromoCodeStore, updatePromoCode, removePromoCode,
          },
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(PromocodeManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('pagedPromoCodes', () => {
    it('should return slice of promo codes', () => {
      expect(component.pagedPromoCodes.length).toBeLessThanOrEqual(component.pageSize);
    });
  });

  describe('toggleAddForm', () => {
    it('should toggle showAddForm', () => {
      expect(component.showAddForm()).toBe(false);
      component.toggleAddForm();
      expect(component.showAddForm()).toBe(true);
    });

    it('should reset form when closing', () => {
      component.showAddForm.set(true);
      component.addForm.patchValue({ promoCode: 'TEST' });
      component.toggleAddForm();
      expect(component.addForm.value.promoCode).toBeFalsy();
    });
  });

  describe('onAddSubmit', () => {
    it('should not call API when form is invalid', () => {
      component.addForm.reset();
      component.onAddSubmit();
      expect(addPromoCode).not.toHaveBeenCalled();
      expect(component.submitted()).toBe(true);
    });

    it('should call addPromoCode API on valid form', () => {
      addPromoCode.mockReturnValue(of({ data: { promoCodeId: 'pc3' } }));
      component.addForm.setValue({
        promoCode: 'NEWCODE',
        discountPercent: 15,
        fromDate: futureDateStr,
        toDate: futureDateStr,
      });
      component.onAddSubmit();
      expect(addPromoCode).toHaveBeenCalled();
    });
  });

  describe('Edit modal', () => {
    it('openEditModal should populate form and show modal', () => {
      component.openEditModal(mockPromos[0]);
      expect(component.editingPromo()).toEqual(mockPromos[0]);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm.value.promoCode).toBe('SAVE10');
    });

    it('closeEditModal should hide modal and clear editingPromo', () => {
      component.openEditModal(mockPromos[0]);
      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
      expect(component.editingPromo()).toBeNull();
    });

    it('onEditSubmit should call editPromoCode API on valid form', () => {
      editPromoCode.mockReturnValue(of({ data: { isSuccess: true } }));
      component.openEditModal(mockPromos[0]);
      component.onEditSubmit();
      expect(editPromoCode).toHaveBeenCalled();
    });

    it('onEditSubmit should not call API when form is invalid', () => {
      component.openEditModal(mockPromos[0]);
      component.editForm.get('promoCode')?.setValue('');
      component.onEditSubmit();
      expect(editPromoCode).not.toHaveBeenCalled();
    });
  });

  describe('Delete', () => {
    it('openDeleteConfirm should set id and show modal', () => {
      component.openDeleteConfirm('pc1');
      expect(component.deletePromoId()).toBe('pc1');
      expect(component.confirmModal()).toBe(true);
    });

    it('cancelDelete should clear id and hide modal', () => {
      component.openDeleteConfirm('pc1');
      component.cancelDelete();
      expect(component.deletePromoId()).toBe('');
      expect(component.confirmModal()).toBe(false);
    });

    it('confirmDelete should call deletePromoCode and update store', () => {
      deletePromoCode.mockReturnValue(of({ data: { isSuccess: true } }));
      component.deletePromoId.set('pc1');
      component.confirmDelete();
      expect(deletePromoCode).toHaveBeenCalled();
      expect(removePromoCode).toHaveBeenCalledWith('pc1');
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
  });

  describe('Status helpers', () => {
    it('isExpired should return true for past date', () => {
      expect(component.isExpired(pastDateStr)).toBe(true);
    });

    it('isExpired should return false for future date', () => {
      expect(component.isExpired(futureDateStr)).toBe(false);
    });

    it('isActive should return true for current promo', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      expect(component.isActive(yesterday.toISOString(), futureDateStr)).toBe(true);
    });

    it('activeCount should count active promos', () => {
      expect(component.activeCount(mockPromos)).toBe(1);
    });

    it('expiredCount should count expired promos', () => {
      expect(component.expiredCount(mockPromos)).toBe(1);
    });
  });
});
