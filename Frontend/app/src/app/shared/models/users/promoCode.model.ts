export class GetAllUserPromoCodesResponseDTO {
  promoCodes: UserPromoCodeItemDTO[] = [];
  totalCount: number = 0;
}

export class UserPromoCodeItemDTO {
  promoCodeId: string = '';
  promoCodeName: string = '';
  discountPercentage: number = 0;
  fromDate: Date = new Date();
  toDate: Date = new Date();
}