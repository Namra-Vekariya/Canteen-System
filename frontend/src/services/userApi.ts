import type { ApiResponse } from '@/types/api';
import { apiClient } from './apiClient';

// Replicating your C# UserProfileResponse DTO
export interface UserProfileResponse {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  role: string;
  profileImageUrl: string | null;
  createdAt: string | null;
}

export const userApi = {
  // GET: api/v1/Users/profile
  getProfile: async () => {
    const response = await apiClient.get<ApiResponse<UserProfileResponse>>('/Users/profile');
    return response.data;
  },

  // PUT: api/v1/Users/profile
  updateProfile: async (data: { name?: string; phone?: string | null }) => {
    const response = await apiClient.put<ApiResponse<UserProfileResponse>>('/Users/profile', data);
    return response.data;
  },

  // PUT: api/v1/Users/profile-image
  uploadProfileImage: async (file: File) => {
    const formData = new FormData();
    formData.append('image', file); // 'image' matches the parameter name in your C# controller

    const response = await apiClient.put<ApiResponse<UserProfileResponse>>('/Users/profile-image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};