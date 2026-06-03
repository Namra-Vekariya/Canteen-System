export interface CreateOrderItemRequest {
  menuItemId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  items: CreateOrderItemRequest[];
}

export interface OrderItemResponse {
  itemName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  isVeg: boolean;
}

export interface CreateOrderResponse {
  orderId: string;
  orderNumber: string;
  totalAmount: number;
  orderStatus: string;
  paymentStatus: string;
  paymentMethod: string;
  items: OrderItemResponse[];
}

export interface OrderResponse {
  orderId: string;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  paymentMethod: string;
  totalAmount: number;
  createdAt: string;
  collectedAt?: string;
  cancelledAt?: string;
  specialInstructions?: string;
  items: OrderItemResponse[];
}