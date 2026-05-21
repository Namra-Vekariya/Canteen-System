import { z } from 'zod';
import { apiClient } from './apiClient';
import type { AuthResponse } from '@/types/api';

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required').email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

export const registerSchema = z.object({
  name: z.string().min(1, 'Full name is required').max(150, 'Name is too long'),
  email: z.string().min(1, 'Email is required').email('Invalid email address').max(255, 'Email is too long'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

export type LoginFormData = z.input<typeof loginSchema>;
export type RegisterFormData = z.input<typeof registerSchema>;

export const authApi = {
  login: async (data: LoginFormData) => {
    const response = await apiClient.post('/auth/login', data);
    return response.data.data as AuthResponse;
  },

  register: async (data: RegisterFormData) => {
    const response = await apiClient.post('/auth/register', data);
    return response.data.data as AuthResponse;
  },

  refresh: async () => {
    const response = await apiClient.post('/auth/refresh');
    return response.data.data as AuthResponse;
  },

  logout: async () => {
    await apiClient.post('/auth/logout');
  },
};
