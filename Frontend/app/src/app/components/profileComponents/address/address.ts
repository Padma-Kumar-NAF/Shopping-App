import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import {
  AddressModel,
  AddressDTO,
  NewAddressResponseDTO,
  DeleteAddressRequestDTO,
  DeleteAddressResponseDTO,
  UpdateAddressResponseDTO,
} from '../../../models/address.model';
import {
  FormControl,
  FormGroup,
  FormsModule,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { toast } from 'ngx-sonner';
import { AddressApiService } from '../../../services/address.service';
import { PaginationModel } from '../../../models/pagination.model';
import { LoaderService } from '../../../services/loading.service';

@Component({
  selector: 'app-address',
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './address.html',
  styleUrl: './address.css',
})
export class Address implements OnInit {

  loader = inject(LoaderService);
  ngOnInit(): void {
    this.getUserAddress();
  }

  constructor() {
    this.formAddress = new AddressDTO();
    this.newAddress = new AddressDTO();
    this.pagination = new PaginationModel();
    this.deleteAddressId = new DeleteAddressRequestDTO();
    this.pagination.PageSize = 10;
    this.pagination.PageNumber = 1;
    this.addressForm = new FormGroup({
      addressLine1: new FormControl('', [Validators.required]),
      addressLine2: new FormControl('', [Validators.required]),
      state: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      pincode: new FormControl('', [Validators.required]),
    });
  }

  private apiService: AddressApiService = inject(AddressApiService);

  addresses: WritableSignal<AddressModel> = signal<AddressModel>(new AddressModel());
  editingId = signal<string | null>(null);
  pagination: PaginationModel;

  newAddress: AddressDTO;
  addressForm: FormGroup;
  deleteAddressId: DeleteAddressRequestDTO;

  showModal = signal(true);
  editMode = signal(false);
  formAddress: AddressDTO;

  confirmModal = signal(false);
  deleteId = signal<string | null>(null);

  isSaveButtonDisabled = signal(false);
  isCancelButtonDisabled = signal(false);

  getUserAddress() {
    this.loader.show()
    this.apiService.GetUserAddresses(this.pagination).subscribe({
      next: (respose: AddressModel) => {
        this.addresses.set(respose);
      },
      error: () => {
        console.error('Failed to fetch addresses');
      },
      complete:()=> {
        console.log("Get user address completed");
      },
      
    });
    this.loader.hide()
  }

  openDeleteConfirm(id: string) {
    this.deleteId.set(id);
    this.confirmModal.set(true);
  }

  cancelDelete() {
    this.confirmModal.set(false);
    this.deleteId.set(null);
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

    this.formAddress = {
      ...this.formAddress,
      ...this.addressForm.value,
    };

    if (this.editMode()) {
      console.log(this.formAddress);

      this.apiService.UpdateAddress(this.formAddress).subscribe({
        next: (response: UpdateAddressResponseDTO) => {
          console.log('response');
          console.log(response);
          toast.dismiss(toastId);
          toast.success('Address updated');

          this.addresses.update((data) => ({
            ...data,
            addressList: data.addressList.map((a) =>
              a.addressId === this.formAddress.addressId ? this.formAddress : a,
            ),
          }));
        },
        error: (err: any) => {
          toast.dismiss(toastId);
          const errorMessage = err?.error?.message || err?.error || 'Something went wrong';
          toast.error(errorMessage);
        },
        complete: () => {
          this.closeModal();
        },
      });
    } else {
      this.apiService.AddNewAddress(this.formAddress).subscribe({
        next: (response: NewAddressResponseDTO) => {
          this.formAddress.addressId = response.addressId;
          this.addresses.update((data) => ({
            ...data,
            addressList: [...data.addressList, this.formAddress],
          }));

          console.log('response');
          console.log(response);

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

  deleteAddress() {
    const addressId = this.deleteId();
    if (!addressId) return;

    this.cancelDelete();

    console.log('addressId');
    console.log(addressId);
    const toastId = toast.loading('Deleting address...');

    this.deleteAddressId.addressId = addressId;

    this.apiService.DeleteAddress(this.deleteAddressId).subscribe({
      next: (response: DeleteAddressResponseDTO) => {
        console.log(response);

        toast.dismiss(toastId);
        toast.success('Address deleted');

        this.addresses.update((data) => ({
          ...data,
          addressList: data.addressList.filter((a) => a.addressId !== addressId),
        }));
      },
      error: (err: any) => {
        toast.dismiss(toastId);
        const errorMessage = err?.error?.message || err?.error || 'Something went wrong';
        toast.error(errorMessage);
      },
      complete: () => {
        console.log('Delete address completed');
      },
    });
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

    this.addressForm.reset();

    this.addressForm.markAsPristine();
    this.addressForm.markAsUntouched();

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
    this.addressForm.patchValue({
      addressLine1: address.addressLine1,
      addressLine2: address.addressLine2,
      city: address.city,
      state: address.state,
      pincode: address.pincode,
    });

    this.showModal.set(false);
  }

  closeModal() {
    this.showModal.set(true);
    this.addressForm.reset();
    this.editMode.set(false);
  }
}
