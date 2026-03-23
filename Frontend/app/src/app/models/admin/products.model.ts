export class GetAllProductsResponseDTO {
  productList: ProductDetails[] = [];
}

export class ProductDetails {
  productId!: string;
  categoryId!: string;
  stockId!: string;
  productName: string = '';
  categoryName: string = '';
  imagePath: string = '';
  description: string = '';
  price: number = 0;
  quantity: number = 0;
  review: ReviewDTO[] = [];
}

export class ReviewDTO {
  summary: string = '';
  reviewPoints: number = 0;
}

// ── Add ───────────────────────────────────────────────────────────────────

export class AddNewProductRequestDTO {
  categoryId: string = '';
  name: string = '';
  imagePath: string = '';
  description: string = '';
  price: number = 0;
  quantity: number = 0;
}

export class AddNewProductResponseDTO {
  productId: string = '';
}

// ── Update ────────────────────────────────────────────────────────────────

export class UpdateProductRequestDTO {
  productId: string = '';
  categoryId: string = '';
  name: string = '';
  imagePath: string = '';
  description: string = '';
  price: number = 0;
  quantity: number = 0;
}

export class UpdateProductResponseDTO {
  isUpdate: boolean = false;
  categoryId: string = '';
}

// ── Delete ────────────────────────────────────────────────────────────────

export class DeleteProductResponseDTO {
  isSuccess: boolean = false;
}