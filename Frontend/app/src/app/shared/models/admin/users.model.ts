export class GetUsersResponseDTO {
  usersList: UserDetailsDTO[] = [];
}

export class UserDetailsDTO {
  userId: string = '';
  userDetailsId: string = '';
  name: string = '';
  email: string = '';
  role: string = '';
  activeStatus: boolean = false;
  phoneNumber: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
}

export interface DeactivateUserResponseDTO {
  unActivated: boolean;
}

export interface ChangeUserRoleResponseDTO {
  isChanged: boolean;
}
