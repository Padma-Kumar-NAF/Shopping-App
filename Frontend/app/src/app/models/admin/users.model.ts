export class GetUsersResponseDTO {
    usersList : UserDetailsDTO[] = []
}

export class UserDetailsDTO {
  userId: string = '';
  userDetails: string = '';
  name: string = '';
  email: string = '';
  role: string = '';
  phoneNumber: string = '';
  addressLine1: string = '';
  addresLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
}
