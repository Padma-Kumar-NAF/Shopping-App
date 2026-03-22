export class GetAllOrderResponseDTO {
  items: OrderDetailsResponseDTO[] = [];
}

export class OrderDetailsResponseDTO {
  orderId: string = '';
  status: string = '';
  totalProductsCount: number = 0;
  totalAmount: number = 0;
  deliveryDate: string = '';
  address: AddressDTO = new AddressDTO();
  payment: PaymentDTO = new PaymentDTO();
  orderBy: OrderBy = new OrderBy();
  items: OrderItemDTO[] = [];
}

export class OrderBy {
  userEmail: string = '';
  userName: string = '';
}

export class AddressDTO {
  addressId: string = '';
  addressLine1: string = '';
  addressLine2: string = '';
  state: string = '';
  city: string = '';
  pincode: string = '';
}

export class OrderItemDTO {
  orderDetailsId: string = '';
  productId: string = '';
  imagePath: string = '';
  productName: string = '';
  quantity: number = 0;
  productPrice: number = 0;
}

export class PaymentDTO {
  paymentId: string = '';
  paymentType: string = '';
}

export class UpdateOrderResponseDTO {
  IsUpdated: boolean = false;
}

export class UpdateOrderRequestDTO {
  OrderId: string = '';
  OrderStatus: string = '';
}