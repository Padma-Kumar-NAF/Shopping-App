export class LoginModel {
  email: string = '';
  password: string = '';
}

export class SignupModel {
  name: string = '';
  email: string = '';
  password: string = '';
  phoneNumber: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
  // userDetails: UserDetails = new UserDetails();
}

// export class UserDetails {
//   phoneNumbe: string = '';
//   addressLine1: string = '';
//   addressLine2: string = '';
//   state: string = '';
//   city: string = '';
//   pincode: string = '';
// }

export class CreateUserResponseDTO {
  userId: string = '';
  name: string = '';
  email: string = '';
  userDetails: CreateUserDetailsDTO = new CreateUserDetailsDTO();
}

export class CreateUserDetailsDTO {
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

export class LoginResponseDTO {
  token : string = ''
}
