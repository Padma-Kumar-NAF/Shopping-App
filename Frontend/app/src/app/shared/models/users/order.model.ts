import { PaginationModel } from "./pagination.model";

export interface GetUserOrderDetailsRequestDTO {
  Pagination: PaginationModel;
}

export interface GetUserOrderDetailsResponseDTO {
  items: OrderDetailsResponseDTO[];
}

export interface OrderDetailsResponseDTO {
  orderId: string;
  status: string;
  totalProductsCount: number;
  totalAmount: number;
  deliveryDate: string;
  isRefunded: boolean;
  address: AddressDTO;
  payment: PaymentDTO;
  orderBy: OrderBy;
  items: OrderItemDTO[];
}

export interface OrderBy {
  userEmail: string;
  userName: string;
}

export interface AddressDTO {
  addressId: string;
  addressLine1: string;
  addressLine2: string;
  state: string;
  city: string;
  pincode: string;
}

export interface OrderItemDTO {
  orderDetailsId: string;
  productId: string;
  imagePath: string;
  productName: string;
  quantity: number;
  productPrice: number;
}

export interface PaymentDTO {
  paymentId: string;
  paymentType: string;
}

export interface PlaceOrderItemDTO {
  productId: string;
  productName: string;
  quantity: number;
  productPrice: number;
}

export interface PlaceOrderRequestDTO {
  addressId: string;
  totalProductsCount: number;
  totalAmount: number;
  orderProductdDetails: PlaceOrderItemDTO;
  paymentType: string;
  promoCode: string;
  useWallet: boolean;
  stripePaymentId: string;
}

export interface PlaceOrderResponseDTO {
  isSuccess: boolean;
  orderId: string;
  paymentId: string;
  deliveryDate: string;
  orderDetailsId: string;
  subtotal: number;
  tax: number;
  shipping: number;
  discountPercentage: number;
  discountAmount: number;
  walletUsed: number;
  finalAmount: number;
}