export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors?: string[];
}

export interface AuthResponse {
    id: string;
    name: string;
    email: string;
    role: string;
    phone: string | null;            
    profileImageUrl: string | null;
    accessToken: string;
}

export interface RegisterResponse {
    message: string;
    email: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    name: string;
    email: string;
    password: string;
}