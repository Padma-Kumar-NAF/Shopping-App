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

export class AddToCartRequestDTO {
  productId: string = '';
  quantity: number = 1;
}

export class AddToCartResponseDTO {
  cartId: string = '';
  cartItemId: string = '';
}

export class UpdateUserCartRequestDTO {
  cartId: string = '';
  cartItemId: string = '';
  productId: string = '';
  quantity: number = 0;
}

export class UpdateUserCartResponseDTO {
  isUpdated: boolean = false;
}

export class OrderAllFromCartRequestDTO {
  cartId: string = '';
  addressId: string = '';
  paymentType: string = '';
  promoCode: string = '';
  useWallet: boolean = false;
  stripePaymentId: string = '';
}

export class OrderAllFromCartResponseDTO {
  isSuccess: boolean = false;
  orderId: string = '';
  subtotal: number = 0;
  tax: number = 0;
  shipping: number = 0;
  discountPercentage: number = 0;
  discountAmount: number = 0;
  walletUsed: number = 0;
  finalAmount: number = 0;
  paymentStatus: string = '';
}

export class RemoveAllFromCartResponseDTO {
  isRemoved: boolean = false;
}

export class RemoveFromCartRequestDTO {
  CartId: string = '';
  CartItemId: string = '';
  ProductId: string = '';
}

export class RemoveFromCartResponseDTO {
  isRemoved: boolean = false;
}
