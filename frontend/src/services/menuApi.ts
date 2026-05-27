import { apiClient } from "./apiClient";

export interface CategoryResponse {
    id: string;
    name: string;
    slug: string;
    description?: string;
    displayOrder: number;
}

export interface MenuItemResponse {
    id: string;
    categoryId: string;
    name: string;
    slug: string;
    description?: string;
    price: number;
    imageUrl?: string;
    isVeg: boolean;
    isAvailable: boolean;
    calories?: string;
    prepTimeMins?: number;
    categoryName?: string;
}

export const menuApi = {
    getCategories: async (): Promise<CategoryResponse[]> => {
        const response = await apiClient.get('Menu/categories');
        return response.data.data; // Unwrapping your ApiResponse<T>
    },

    getMenuItems: async (categoryId?: string): Promise<MenuItemResponse[]> => {
        const url = categoryId ? `Menu/items?categoryId=${categoryId}` : 'Menu/items';
        const response = await apiClient.get(url);
        return response.data.data;
    }
};