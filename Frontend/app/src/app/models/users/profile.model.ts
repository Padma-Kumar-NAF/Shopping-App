export class GetUserByIdResponseDTO {
  userId: string = '';
  name: string = '';
  email: string = '';
  userDetails: GetUserDetailsDTO = new GetUserDetailsDTO();
}

export class GetUserDetailsDTO {
  userDetailsId: string = '';
  name: string = '';
  email: string = '';
  phoneNumber: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
}
//////////////////////////////
export class EditUserDetailsModel {
  Details: udpateUserDetailsDTO = new udpateUserDetailsDTO();
}

class udpateUserDetailsDTO {
  name: string = '';
  phoneNumber: string = '';
  city: string = '';
  state: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  pincode: string = '';
}

export class newEmailRequestDTO {
  oldEmail: string = '';
  newEmail: string = '';
  password: string = '';
}

export class EditMailResponseDTOModel {
  isSuccess: boolean = false;
}