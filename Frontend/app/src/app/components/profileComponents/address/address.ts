import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { AddressModel, AddressDTO } from '../../../models/address.model';
import {
  FormControl,
  FormGroup,
  FormsModule,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { toast } from 'ngx-sonner';
import { AddressApiService } from '../../../services/address.service';
import { PaginationModel } from '../../../models/pagination.model';

@Component({
  selector: 'app-address',
  imports: [FormsModule, MatIcon, ReactiveFormsModule],
  templateUrl: './address.html',
  styleUrl: './address.css',
})
export class Address implements OnInit {
  ngOnInit(): void {
    this.getUserAddress();
  }

  constructor() {
    this.formAddress = new AddressDTO();
    this.newAddress = new AddressDTO();
    this.pagination = new PaginationModel();
    this.pagination.PageSize = 10;
    this.pagination.PageNumber = 1;
    this.addressForm = new FormGroup({
      addressLine1: new FormControl('Test line 1', [Validators.required]),
      addressLine2: new FormControl('Test line 1', [Validators.required]),
      state: new FormControl('state', [Validators.required]),
      city: new FormControl('city', [Validators.required]),
      pincode: new FormControl('654332', [Validators.required]),
    });
  }

  private apiService: AddressApiService = inject(AddressApiService);

  addresses: WritableSignal<AddressModel> = signal<AddressModel>(new AddressModel());
  editingId = signal<string | null>(null);
  pagination: PaginationModel;

  newAddress: AddressDTO;
  addressForm: FormGroup;

  showModal = signal(true);
  editMode = signal(false);
  formAddress: AddressDTO;

  isSaveButtonDisabled = signal(false);
  isCancelButtonDisabled = signal(false);

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

  submitAddress() {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      toast.error('Please fill all required fields correctly');
      return;
    }

    const toastId = toast.loading('Saving address...');

    this.isSaveButtonDisabled.set(true);
    this.isCancelButtonDisabled.set(true);

    this.formAddress = this.addressForm.value;

    if (this.editMode()) {
      this.addresses.update((data) => ({
        ...data,
        addressList: data.addressList.map((a) =>
          a.addressId === this.formAddress.addressId ? this.formAddress : a,
        ),
      }));

      toast.dismiss(toastId);
      toast.success('Address updated');

      this.closeModal();
    } else {
      this.apiService.AddNewAddress(this.formAddress).subscribe({
        next: (response: any) => {
          this.addresses.update((data) => ({
            ...data,
            addressList: [...data.addressList, response],
          }));

          toast.dismiss(toastId);
          toast.success('Address added');

          this.closeModal();
        },

        error: (err: any) => {
          toast.dismiss(toastId);
          const errorMessage = err?.error?.message || err?.error || 'Something went wrong';
          toast.error(errorMessage);
        },

        complete: () => {
          this.isSaveButtonDisabled.set(false);
          this.isCancelButtonDisabled.set(false);
        },
      });
    }
  }

  deleteAddress(id: string) {
    this.addresses.update((data) => ({
      ...data,
      addressList: data.addressList.filter((a) => a.addressId !== id),
    }));

    toast.success('Address deleted');
  }

  editAddress(address: AddressDTO) {
    this.editingId.set(address.addressId);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  saveAddress(updated: AddressDTO) {
    this.addresses.update((data) => ({
      ...data,
      addressList: data.addressList.map((a) => (a.addressId === updated.addressId ? updated : a)),
    }));

    this.editingId.set(null);
    toast.success('Address saved');
  }

  openAddModal() {
    this.editMode.set(false);

    this.formAddress = {
      addressId: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      pincode: '',
    };

    this.showModal.set(false);
  }

  openEditModal(address: AddressDTO) {
    this.editMode.set(true);

    this.formAddress = { ...address };

    this.showModal.set(false);
  }

  closeModal() {
    this.showModal.set(true);
  }
}
