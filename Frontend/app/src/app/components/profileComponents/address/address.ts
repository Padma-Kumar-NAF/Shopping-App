import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { AddressModel, AddressDTO } from '../../../models/address.model';
import { FormsModule } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { toast } from 'ngx-sonner';
import { AddressApiService } from '../../../services/address.service';
import { PaginationModel } from '../../../models/pagination.model';

@Component({
  selector: 'app-address',
  imports: [FormsModule, MatIcon],
  templateUrl: './address.html',
  styleUrl: './address.css',
})
export class Address implements OnInit {

  private apiService: AddressApiService = inject(AddressApiService);

  ngOnInit(): void {
    this.getUserAddress();
  }

  constructor() {
    this.pagination = new PaginationModel();
    this.pagination.Limit = 10;
    this.pagination.PageNumber = 1;
  }

  // =========================
  // EXISTING CODE (UNCHANGED)
  // =========================

  addresses: WritableSignal<AddressModel> = signal<AddressModel>(new AddressModel());

  editingId = signal<string | null>(null);

  pagination: PaginationModel;

  getUserAddress() {
    this.apiService.GetUserAddresses(this.pagination).subscribe({
      next: (respose: AddressModel) => {
        this.addresses.set(respose);
        console.log('Address List', this.addresses());
      },
      error: () => {
        console.error('Failed to fetch addresses');
      },
      complete() {
        console.log('Address API Completed');
      },
    });
  }

  editAddress(address: AddressDTO) {
    this.editingId.set(address.addressId);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  deleteAddress(id: string) {
    this.addresses.update((data) => ({
      ...data,
      addressList: data.addressList.filter((a) => a.addressId !== id),
    }));

    toast.success('Address deleted');
  }

  addNewAddress() {
    const newAddress: AddressDTO = {
      addressId: crypto.randomUUID(),
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      pincode: '',
    };

    this.addresses.update((data) => ({
      ...data,
      addressList: [...data.addressList, newAddress],
    }));

    this.editingId.set(newAddress.addressId);
  }

  saveAddress(updated: AddressDTO) {
    this.addresses.update((data) => ({
      ...data,
      addressList: data.addressList.map((a) =>
        a.addressId === updated.addressId ? updated : a
      ),
    }));

    this.editingId.set(null);
    toast.success('Address saved');
  }

  // ====================================================
  // NEW CODE (MODAL SUPPORT FOR ADD + EDIT)
  // ====================================================

  showModal = signal(false);

  editMode = signal(false);

  formAddress: AddressDTO = {
    addressId: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    pincode: '',
  };

  // OPEN ADD MODAL
  openAddModal() {
    this.editMode.set(false);

    this.formAddress = {
      addressId: crypto.randomUUID(),
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      pincode: '',
    };

    this.showModal.set(true);
  }

  // OPEN EDIT MODAL
  openEditModal(address: AddressDTO) {
    this.editMode.set(true);

    this.formAddress = { ...address };

    this.showModal.set(true);
  }

  // CLOSE MODAL
  closeModal() {
    this.showModal.set(false);
  }

  // SUBMIT ADD OR EDIT
  submitAddress() {

    if (this.editMode()) {

      // UPDATE EXISTING
      this.addresses.update((data) => ({
        ...data,
        addressList: data.addressList.map((a) =>
          a.addressId === this.formAddress.addressId ? this.formAddress : a
        ),
      }));

      toast.success('Address updated');

    } else {

      // ADD NEW
      this.addresses.update((data) => ({
        ...data,
        addressList: [...data.addressList, this.formAddress],
      }));

      toast.success('Address added');

    }

    this.closeModal();
  }
}