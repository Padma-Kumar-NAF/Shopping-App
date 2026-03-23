import { Injectable, signal } from '@angular/core';
import { AddressDTO } from '../models/users/address.model';

@Injectable({
  providedIn: 'root',
})
export class AddressSelectionService {
  private selectedAddress = signal<AddressDTO | null>(null);
  private availableAddresses = signal<AddressDTO[]>([]);

  setSelectedAddress(address: AddressDTO): void {
    this.selectedAddress.set(address);
  }

  getSelectedAddress(): AddressDTO | null {
    return this.selectedAddress();
  }

  clearSelectedAddress(): void {
    this.selectedAddress.set(null);
  }

  setAvailableAddresses(addresses: AddressDTO[]): void {
    this.availableAddresses.set(addresses);
  }

  getAvailableAddresses(): AddressDTO[] {
    return this.availableAddresses();
  }

  hasSelectedAddress(): boolean {
    return this.selectedAddress() !== null;
  }
}
