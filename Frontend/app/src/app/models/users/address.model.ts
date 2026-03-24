export class AddressDTO {
  addressId: string = "";
  addressLine1: string = "";
  addressLine2: string = "";
  state: string = "";
  city: string = "";
  pincode: string = "";
}

export class AddressModel {
  addressList: AddressDTO[] = [];
}

export class NewAddressResponseDTO{
  addressId: string = "";
}

export class DeleteAddressResponseDTO{
  isSuccess : boolean = false;
}

export class DeleteAddressRequestDTO{
  addressId: string = "";
}

export class UpdateAddressResponseDTO{
  isSuccess : boolean = false;
}

// CreateNewAddressRequestDTO
// public Guid UserId { get; set; }
// public string AddressLine1 { get; set; } = string.Empty;
// public string AddressLine2 { get; set; } = string.Empty;
// public string State { get; set; } = string.Empty;
// public string City { get; set; } = string.Empty;
// public string PinCode { get; set; } = string.Empty;

// CreateNewAddressResponseDTO
//  public Guid AddressId { get; set; }
//  public Guid UserId { get; set; }
//  public string AddressLine1 { get; set; } = string.Empty;
//  public string AddressLine2 { get; set; } = string.Empty;
//  public string State { get; set; } = string.Empty;
//  public string City { get; set; } = string.Empty;
//  public string PinCode { get; set; } = string.Empty;