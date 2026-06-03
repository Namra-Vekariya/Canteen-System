import { apiClient } from './apiClient';
import type { CreateOrderRequest, CreateOrderResponse, OrderResponse } from '@/types/order';

export const orderApi = {
  placeOrder: async (data: CreateOrderRequest): Promise<CreateOrderResponse> => {
    const response = await apiClient.post('/Orders/checkout', data);
    return response.data.data;
  },
  
  getOrderById: async (orderId: string): Promise<OrderResponse> => {
    const response = await apiClient.get(`/Orders/${orderId}`);
    return response.data.data;
  },
};
