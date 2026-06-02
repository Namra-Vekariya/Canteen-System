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
