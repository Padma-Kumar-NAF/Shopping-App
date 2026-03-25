export interface PromoCodeItemDTO {
  promoCodeId: string;
  promoCodeName: string;
  discountPercentage: number;
  fromDate: string;
  toDate: string;
}

export interface GetAllPromocodesResponseDTO {
  promoCodes: PromoCodeItemDTO[];
  totalCount: number;
}

export interface GetAllPromocodesRequestDTO {
  pagination: { pageNumber: number; pageSize: number };
}

export interface AddPromoCodeRequestDTO {
  promoCodeName: string;
  discountPercentage: number;
  fromDate: string;
  toDate: string;
}

export interface AddPromoCodeResponseDTO {
  promoCodeId: string;
}

export interface EditPromoCodeRequestDTO {
  promoCodeId: string;
  promoCodeName: string;
  discountPercentage: number;
  fromDate: string;
  toDate: string;
}

export interface EditPromoCodeResponseDTO {
  isSuccess: boolean;
}

export interface DeletePromoCodeRequestDTO {
  promoCodeId: string;
}

export interface DeletePromoCodeResponseDTO {
  isSuccess: boolean;
}

export interface ValidatePromoCodeRequestDTO {
  promoCodeName: string;
}

export interface ValidatePromoCodeResponseDTO {
  isValid: boolean;
  discountPercentage: number;
  promoCodeId: string;
  message: string;
}
