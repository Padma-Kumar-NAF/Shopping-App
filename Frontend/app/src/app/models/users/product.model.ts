// Mirrors backend GetAllProductsResponseDTO / ProductDetails
export interface ReviewDTO {
  summary: string;
  reviewPoints: number;
}

export interface ProductDetails {
  productId: string;
  categoryId: string;
  stockId: string;
  productName: string;
  categoryName: string;
  imagePath: string;
  description: string;
  price: number;
  quantity: number;
  review: ReviewDTO[];
}

export interface GetAllProductsResponseDTO {
  productList: ProductDetails[];
}

export interface GetAllProductsRequestDTO {
  pagination: {
    pageSize: number;
    pageNumber: number;
  };
}

export interface SearchProductByNameRequestDTO {
  productName: string;
}

export interface SearchProductByNameResponseDTO {
  productsList: ProductDetails[];
}

export interface SearchProductByIdRequestDTO {
  productId: string;
}

export interface SearchProductByIdResponseDTO {
  productId: string;
  categoryId: string;
  stockId: string;
  productName: string;
  categoryName: string;
  imagePath: string;
  description: string;
  price: number;
  quantity: number;
  review: ReviewDTO[];
}
