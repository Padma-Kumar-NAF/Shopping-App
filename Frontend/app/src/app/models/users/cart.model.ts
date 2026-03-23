export class GetCartResponseDTO {
  cartId: string = '';
  cartItems: CartItemDTO[] = [];
}

export class CartItemDTO {
  cartItemId: string = '';
  productId: string = '';
  categoryId: string = '';
  productName: string = '';
  imagePath: string = '';
  description: string = '';
  price: number = 0;
  quantity: number = 0;
}

export class RemoveAllFromCartResponseDTO {
  IsRemoved: boolean = false;
}

export class RemoveFromCartRequestDTO {
  CartId: string = '';
  CartItemId: string = '';
  ProductId: string = '';
}

export class RemoveFromCartResponseDTO {
  IsRemoved: boolean = false;
}
