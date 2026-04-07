export interface UserMonthlyProductLimitDTO {
  id: string;
  productId: string;
  productName: string;
  monthlyLimit: number;
  createdAt: string;
}

export interface GetAllLimitsResponseDTO {
  records: UserMonthlyProductLimitDTO[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface AddLimitRequestDTO {
  productId: string;
  monthlyLimit: number;
}

export interface AddLimitResponseDTO {
  id: string;
}

export interface EditLimitRequestDTO {
  id: string;
  monthlyLimit: number;
}

export interface EditLimitResponseDTO {
  isSuccess: boolean;
}

export interface DeleteLimitRequestDTO {
  id: string;
}

export interface DeleteLimitResponseDTO {
  isSuccess: boolean;
}
