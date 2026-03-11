
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