import { apiClient } from './apiClient';
import type { AuthResponse, RegisterResponse } from '@/types/api';
import type { ForgotPasswordFormData, LoginFormData, RegisterFormData, ResetPasswordFormData, VerifyEmailFormData } from '@/schemas/auth';

export const authApi = {
  login: async (data: LoginFormData) => {
    const response = await apiClient.post('/auth/login', data);
    return response.data.data as AuthResponse;
  },

  register: async (data: RegisterFormData) => {
    const response = await apiClient.post('/auth/register', data);
    return response.data.data as RegisterResponse;
  },

  verifyEmail: async (data: VerifyEmailFormData) => {
    const response = await apiClient.post('/auth/verify-email', data);
    return response.data.data as AuthResponse;
  },

  resendVerificationOtp: async (email: string) => {
    const response = await apiClient.post('/auth/resend-verification-otp', { email });
    return response.data.message as string;
  },

  forgotPassword: async (data: ForgotPasswordFormData) => {
    const response = await apiClient.post('/auth/forgot-password', data);
    return response.data.message as string;
  },

  resetPassword: async (data: ResetPasswordFormData) => {
    const response = await apiClient.post('/auth/reset-password', data);
    return response.data.message as string;
  },

  refresh: async () => {
    const response = await apiClient.post('/auth/refresh');
    return response.data.data as AuthResponse;
  },

  logout: async () => {
    await apiClient.post('/auth/logout');
  },
};
