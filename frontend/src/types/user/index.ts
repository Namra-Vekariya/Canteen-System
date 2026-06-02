export interface UserProfileResponse {
    id: string;
    name: string;
    email: string;
    phone: string | null;
    role: string;
    profileImageUrl: string | null;
    createdAt: string | null;
}
