import {
  Component,
  inject,
  OnInit,
  signal,
  WritableSignal,
  Output,
  EventEmitter,
  Input,
} from '@angular/core';
import {
  AddressModel,
  AddressDTO,
  NewAddressResponseDTO,
  DeleteAddressRequestDTO,
  DeleteAddressResponseDTO,
  UpdateAddressResponseDTO,
} from '../../../models/users/address.model';
import {
  FormControl,
  FormGroup,
  FormsModule,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { toast } from 'ngx-sonner';
import { AddressApiService } from '../../../services/userServices/address.service';
import { AddressSelectionService } from '../../../services/address-selection.service';
import { PaginationModel } from '../../../models/users/pagination.model';
import { LoaderService } from '../../../services/loading.service';
import { CommonModule } from '@angular/common';
import { ApiResponse } from '../../../models/users/apiResponse.model';

@Component({
  selector: 'app-address',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './address.html',
  styleUrl: './address.css',
})
export class Address implements OnInit {
  @Input() selectionMode = false;
  @Output() addressSelected = new EventEmitter<AddressDTO>();

  // loader = inject(LoaderService);
  private apiService: AddressApiService = inject(AddressApiService);
  private addressSelectionService = inject(AddressSelectionService);

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

  ngOnInit(): void {
    this.getUserAddress();
  }

  constructor() {
    this.formAddress = new AddressDTO();
    this.newAddress = new AddressDTO();
    this.pagination = new PaginationModel();
    this.deleteAddressId = new DeleteAddressRequestDTO();
    this.pagination.pageSize = 10;
    this.pagination.pageNumber = 1;
    this.addressForm = new FormGroup({
      addressLine1: new FormControl('', [Validators.required]),
      addressLine2: new FormControl('', [Validators.required]),
      state: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      pincode: new FormControl('', [Validators.required]),
    });
  }

  selectAddress(address: AddressDTO): void {
    if (this.selectionMode) {
      this.addressSelectionService.setSelectedAddress(address);
      this.addressSelected.emit(address);
      toast.success('Address selected');
    }
  }

  getUserAddress() {
    // this.loader.show();
    this.apiService.GetUserAddresses(this.pagination).subscribe({
      next: (response: ApiResponse<AddressModel>) => {
        console.log('response', response);
        if (response?.data) {
          this.addresses.set(response.data);
          this.addressSelectionService.setAvailableAddresses(response.data.addressList);
        }
      },
      error: () => {
        console.error('Failed to fetch addresses');
        // this.loader.hide();
      },
      complete: () => {
        // this.loader.hide();
        console.log('Get user address completed');
      },
    });
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
      this.apiService.UpdateAddress(this.formAddress).subscribe({
        next: (response: ApiResponse<UpdateAddressResponseDTO>) => {
          console.log('Update response', response);
          toast.dismiss(toastId);
          toast.success('Address updated');

          this.addresses.update((data) => ({
            ...data,
            addressList: data.addressList.map((a) =>
              a.addressId === this.formAddress.addressId ? this.formAddress : a
            ),
          }));
        },
        error: (err: any) => {
          toast.dismiss(toastId);
          const errorMessage = err?.error?.message || err?.error || 'Something went wrong';
          toast.error(errorMessage);
          this.isSaveButtonDisabled.set(false);
          this.isCancelButtonDisabled.set(false);
        },
        complete: () => {
          this.isSaveButtonDisabled.set(false);
          this.isCancelButtonDisabled.set(false);
          this.closeModal();
        },
      });
    } else {
      this.apiService.AddNewAddress(this.formAddress).subscribe({
        next: (response: ApiResponse<NewAddressResponseDTO>) => {
          if (response.data) {
            this.formAddress.addressId = response.data.addressId;
            this.addresses.update((data) => ({
              ...data,
              addressList: [...data.addressList, this.formAddress],
            }));
          }
          console.log('Add response', response);
          toast.dismiss(toastId);
          toast.success('Address added');
          this.closeModal();
        },
        error: (err: any) => {
          toast.dismiss(toastId);
          const errorMessage = err?.error?.message || err?.error || 'Something went wrong';
          toast.error(errorMessage);
          this.isSaveButtonDisabled.set(false);
          this.isCancelButtonDisabled.set(false);
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

    const toastId = toast.loading('Deleting address...');
    this.deleteAddressId.addressId = addressId;

    this.apiService.DeleteAddress(this.deleteAddressId).subscribe({
      next: (response: ApiResponse<DeleteAddressResponseDTO>) => {
        console.log('Delete response', response);
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
    this.isSaveButtonDisabled.set(false);
    this.isCancelButtonDisabled.set(false);
  }
}