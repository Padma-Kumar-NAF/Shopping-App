export enum OrderStatus {
  NOT_DELIVERED = 'Not Delivered',
  SHIPPED = 'Shipped',
  DELIVERED = 'Delivered',
  CANCELLED = 'Cancelled',
}

export const CANCELLABLE_ORDER_STATUSES: OrderStatus[] = [
  OrderStatus.NOT_DELIVERED,
  OrderStatus.SHIPPED,
];

export const NON_CANCELLABLE_ORDER_STATUSES: OrderStatus[] = [
  OrderStatus.DELIVERED,
  OrderStatus.CANCELLED,
];

export function isOrderCancellable(status: string): boolean {
  return CANCELLABLE_ORDER_STATUSES.includes(status as OrderStatus);
}

export function isOrderCancelled(status: string): boolean {
  return status === OrderStatus.CANCELLED;
}

export function isOrderDelivered(status: string): boolean {
  return status === OrderStatus.DELIVERED;
}
