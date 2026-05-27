import type { MenuItemResponse } from '@/services/menuApi';
import { create } from 'zustand';

interface CartItem {
    menuItem: MenuItemResponse;
    quantity: number;
}

interface CartState {
    items: Record<string, CartItem>;
    totalItems: number;
    totalPrice: number;
    addItem: (item: MenuItemResponse) => void;
    removeItem: (itemId: string) => void;
    updateQuantity: (itemId: string, delta: number) => void;
    clearCart: () => void;
}

export const useCartStore = create<CartState>((set, get) => ({
    items: {},
    totalItems: 0,
    totalPrice: 0,

    addItem: (item) => {
        const { items } = get();
        const existing = items[item.id];
        
        if (existing) {
            get().updateQuantity(item.id, 1);
        } else {
            set((state) => {
                const newItems = { ...state.items, [item.id]: { menuItem: item, quantity: 1 } };
                return {
                    items: newItems,
                    totalItems: state.totalItems + 1,
                    totalPrice: state.totalPrice + item.price,
                };
            });
        }
    },

    updateQuantity: (itemId, delta) => {
        set((state) => {
            const currentItem = state.items[itemId];
            if (!currentItem) return state;

            const newQuantity = currentItem.quantity + delta;
            
            if (newQuantity <= 0) {
                const { [itemId]: removed, ...rest } = state.items;
                return {
                    items: rest,
                    totalItems: state.totalItems - currentItem.quantity,
                    totalPrice: state.totalPrice - (currentItem.menuItem.price * currentItem.quantity),
                };
            }

            return {
                items: {
                    ...state.items,
                    [itemId]: { ...currentItem, quantity: newQuantity }
                },
                totalItems: state.totalItems + delta,
                totalPrice: state.totalPrice + (currentItem.menuItem.price * delta),
            };
        });
    },

    removeItem: (itemId) => {
        set((state) => {
            const currentItem = state.items[itemId];
            if (!currentItem) return state;
            
            const { [itemId]: removed, ...rest } = state.items;
            return {
                items: rest,
                totalItems: state.totalItems - currentItem.quantity,
                totalPrice: state.totalPrice - (currentItem.menuItem.price * currentItem.quantity),
            };
        });
    },

    clearCart: () => set({ items: {}, totalItems: 0, totalPrice: 0 }),
}));