export class UserProfile {
  userId: string = '';
  name: string = '';
  email: string = '';
  userDetails: UserDetailsDTO = new UserDetailsDTO();
}

class UserDetailsDTO {
  userId: string = '';
  name: string = '';
  email: string = '';
  phoneNumber: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
}

export class EditUserDetailsModel {
   Details : udpateUserDetailsDTO = new udpateUserDetailsDTO();
}

class udpateUserDetailsDTO {
  name: string = '';
  phoneNumber: string = '';
  city: string = '';
  state: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
}

// export class EditMailModel {
//   oldMail: string = '';
//   newEmail: string = '';
//   password: string = '';
// }

export class EditMailRequestDTOModel{
  isSuccess : boolean = false;
}

export class newEmailRequestDTO{
  oldEmail: string = '';
  newEmail: string = '';
  password: string = '';
}