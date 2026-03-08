import { Component , signal } from '@angular/core';
import { AddressModel } from '../../../models/address.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-address',
  imports: [FormsModule],
  templateUrl: './address.html',
  styleUrl: './address.css',
})
export class Address  {
  addresses = signal<AddressModel[]>([
    {
      id: '1',
      addressLine1: '12 Anna Street',
      addressLine2: 'Near Bus Stand',
      city: 'Chennai',
      state: 'Tamil Nadu',
      pincode: '600001',
    },
    {
      id: '2',
      addressLine1: '45 Gandhi Road',
      addressLine2: 'Opp Mall',
      city: 'Coimbatore',
      state: 'Tamil Nadu',
      pincode: '641001',
    },
  ]);

  editingId = signal<string | null>(null);

  editAddress(address: AddressModel) {
    this.editingId.set(address.id);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  deleteAddress(id: string) {
    this.addresses.update((list) => list.filter((a) => a.id !== id));
  }

  addNewAddress() {
  // Create new empty address or navigate to add form
  // Example: this.router.navigate(['/add-address']);
}


  saveAddress(updated: AddressModel) {
    this.addresses.update((list) => list.map((a) => (a.id === updated.id ? updated : a)));
    this.editingId.set(null);
  }
}
