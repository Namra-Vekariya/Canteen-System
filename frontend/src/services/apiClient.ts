import axios from 'axios';
import { useAuthStore } from '@/store/authStore';
import { useCartStore } from '@/store/cartStore';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5112/api/v1';

//  Create the central instance
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, 
  headers: {
    'Content-Type': 'application/json',
  },
});

// Variable to prevent multiple refresh calls from firing at the exact same time
let isRefreshing = false;
let failedQueue: Array<{ resolve: (value?: unknown) => void; reject: (reason?: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Request Interceptor: Attach the Access Token
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response Interceptor: Handle 401s and Token Refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      // Don't attempt refresh on auth endpoints — let the error propagate
      const skipRefreshUrls = [
        '/auth/login',
        '/auth/register',
        '/auth/verify-email',
        '/auth/resend-verification-otp',
        '/auth/forgot-password',
        '/auth/reset-password',
      ];
      if (skipRefreshUrls.some(url => originalRequest.url?.includes(url))) {
        return Promise.reject(error);
      }
      // If we are already in the middle of refreshing, queue this request up
      if (isRefreshing) {
        return new Promise(function (resolve, reject) {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = 'Bearer ' + token;
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Hit the refresh endpoint (the cookie is sent automatically)
        const refreshResponse = await axios.post(
          `${API_BASE_URL}/auth/refresh`,
          {},
          { withCredentials: true }
        );

        const { accessToken, ...user } = refreshResponse.data.data;

        useAuthStore.getState().setAuth(user, accessToken);

        processQueue(null, accessToken);

        // Retry the original request
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
        
      } catch (refreshError) {
        processQueue(refreshError, null);
        // If refresh fails, wipe auth state — ProtectedRoute will redirect to /login
        useAuthStore.getState().clearAuth();
        useCartStore.getState().clearCart();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);