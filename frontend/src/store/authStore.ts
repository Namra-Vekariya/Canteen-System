import type { AuthResponse } from "@/types/auth";
import { create } from "zustand";

interface AuthState {
    user: Omit<AuthResponse, 'accessToken'> | null;
    accessToken: string | null;
    isAuthenticated: boolean;
    isInitialized: boolean; 
    setAuth: (user: Omit<AuthResponse, 'accessToken'>, token: string) => void;
    clearAuth: () => void;
    setInitialized: () => void;
  }

export const useAuthStore = create<AuthState>((set) => ({
    user: null,
    accessToken: null,
    isAuthenticated: false,
    isInitialized: false,
  
    setAuth: (user, token) => set({ 
      user, 
      accessToken: token, 
      isAuthenticated: true 
    }),
    
    clearAuth: () => set({ 
      user: null, 
      accessToken: null, 
      isAuthenticated: false 
    }),
  
    setInitialized: () => set({ isInitialized: true })
  }));