export class AddPromoCodeRequestDTO {
  PromoCodeName: string = '';
  discountPercentage: number = 0;
  fromDate: string = '';
  toDate: string = '';
}

export class AddPromoCodeResponseDTO {
  promoCodeId: string = '';
  isSuccess: boolean = false;
}

export class ValidatePromoCodeRequestDTO {
  promoCodeName: string = '';
}

export class ValidatePromoCodeResponseDTO {
  isValid: boolean = false;
  discountPercentage: number = 0;
  promoCodeId: string = '';
}
