export class AddressDTO {
  addressId: string = "";
  addressLine1: string = "";
  addressLine2: string = "";
  state: string = "";
  city: string = "";
  pincode: string = "";
}

export class AddressModel {
  userId: string = "";
  addressList: AddressDTO[] = [];
}