import { apiClient } from "./apiClient";
import type { CategoryResponse, MenuItemResponse } from "@/types/menu";

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
