import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Address } from './address';
import { AddressApiService } from '../../../services/userServices/address.service';
import { AddressSelectionService } from '../../../services/address-selection.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('Address', () => {
  let component: Address;
  let fixture: ComponentFixture<Address>;

  const GetUserAddresses = vi.fn();
  const AddNewAddress = vi.fn();
  const UpdateAddress = vi.fn();
  const DeleteAddress = vi.fn();
  const setSelectedAddress = vi.fn();
  const setAvailableAddresses = vi.fn();

  const mockAddresses: any[] = [
    { addressId: 'a1', addressLine1: '123 Main St', addressLine2: 'Apt 1', city: 'Chennai', state: 'TN', pincode: '600001' },
    { addressId: 'a2', addressLine1: '456 Park Ave', addressLine2: '', city: 'Mumbai', state: 'MH', pincode: '400001' },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    GetUserAddresses.mockReturnValue(of({ data: { addressList: mockAddresses } }));

    await TestBed.configureTestingModule({
      imports: [Address],
      providers: [
        { provide: AddressApiService, useValue: { GetUserAddresses, AddNewAddress, UpdateAddress, DeleteAddress } },
        { provide: AddressSelectionService, useValue: { setSelectedAddress, setAvailableAddresses } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Address);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load addresses on init', () => {
    expect(GetUserAddresses).toHaveBeenCalled();
    expect(component.addresses().addressList).toEqual(mockAddresses);
  });

  describe('selectAddress', () => {
    it('should emit addressSelected and call service when in selectionMode', () => {
      component.selectionMode = true;
      const emitted: any[] = [];
      component.addressSelected.subscribe((a: any) => emitted.push(a));
      component.selectAddress(mockAddresses[0]);
      expect(setSelectedAddress).toHaveBeenCalledWith(mockAddresses[0]);
      expect(emitted).toContain(mockAddresses[0]);
    });

    it('should not emit when not in selectionMode', () => {
      component.selectionMode = false;
      const emitted: any[] = [];
      component.addressSelected.subscribe((a: any) => emitted.push(a));
      component.selectAddress(mockAddresses[0]);
      expect(emitted.length).toBe(0);
    });
  });

  describe('openAddModal', () => {
    it('should set editMode to false and reset form', () => {
      component.editMode.set(true);
      component.openAddModal();
      expect(component.editMode()).toBe(false);
      expect(component.showModal()).toBe(false);
    });
  });

  describe('openEditModal', () => {
    it('should set editMode to true and patch form', () => {
      component.openEditModal(mockAddresses[0]);
      expect(component.editMode()).toBe(true);
      expect(component.addressForm.value.city).toBe('Chennai');
      expect(component.showModal()).toBe(false);
    });
  });

  describe('closeModal', () => {
    it('should reset form and set showModal to true', () => {
      component.showModal.set(false);
      component.closeModal();
      expect(component.showModal()).toBe(true);
      expect(component.editMode()).toBe(false);
    });
  });

  describe('submitAddress', () => {
    it('should not call API when form is invalid', () => {
      component.addressForm.reset();
      component.submitAddress();
      expect(AddNewAddress).not.toHaveBeenCalled();
    });

    it('should call AddNewAddress when in add mode', () => {
      AddNewAddress.mockReturnValue(of({ data: { addressId: 'a3' } }));
      component.editMode.set(false);
      component.addressForm.setValue({
        addressLine1: '789 New St',
        addressLine2: 'Suite 2',
        city: 'Delhi',
        state: 'DL',
        pincode: '110001',
      });
      component.submitAddress();
      expect(AddNewAddress).toHaveBeenCalled();
    });

    it('should call UpdateAddress when in edit mode', () => {
      UpdateAddress.mockReturnValue(of({}));
      component.editMode.set(true);
      component.formAddress = { ...mockAddresses[0] };
      component.addressForm.setValue({
        addressLine1: 'Updated St',
        addressLine2: '',
        city: 'Chennai',
        state: 'TN',
        pincode: '600001',
      });
      component.submitAddress();
      expect(UpdateAddress).toHaveBeenCalled();
    });
  });

  describe('Delete', () => {
    it('openDeleteConfirm should set deleteId and show modal', () => {
      component.openDeleteConfirm('a1');
      expect(component.deleteId()).toBe('a1');
      expect(component.confirmModal()).toBe(true);
    });

    it('cancelDelete should clear deleteId and hide modal', () => {
      component.openDeleteConfirm('a1');
      component.cancelDelete();
      expect(component.deleteId()).toBeNull();
      expect(component.confirmModal()).toBe(false);
    });

    it('deleteAddress should call DeleteAddress API and remove from list', () => {
      DeleteAddress.mockReturnValue(of({ data: {} }));
      component.deleteId.set('a1');
      component.deleteAddress();
      expect(DeleteAddress).toHaveBeenCalled();
      expect(component.addresses().addressList.find((a: any) => a.addressId === 'a1')).toBeUndefined();
    });
  });

  describe('editAddress / cancelEdit / saveAddress', () => {
    it('editAddress should set editingId', () => {
      component.editAddress(mockAddresses[0]);
      expect(component.editingId()).toBe('a1');
    });

    it('cancelEdit should clear editingId', () => {
      component.editingId.set('a1');
      component.cancelEdit();
      expect(component.editingId()).toBeNull();
    });

    it('saveAddress should update address in list', () => {
      const updated = { ...mockAddresses[0], city: 'Bangalore' };
      component.saveAddress(updated);
      expect(component.addresses().addressList.find((a: any) => a.addressId === 'a1')?.city).toBe('Bangalore');
    });
  });
});
