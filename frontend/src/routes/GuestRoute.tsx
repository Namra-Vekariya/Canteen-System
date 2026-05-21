import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

export default function GuestRoute() {
  const { isAuthenticated, isInitialized } = useAuthStore();

  // Wait for the silent refresh to finish
  if (!isInitialized) {
    return (
      <div className="flex h-screen w-screen items-center justify-center">
        <div className="animate-pulse text-lg font-medium text-gray-500">
          Loading...
        </div>
      </div>
    );
  }

  // If they ARE logged in, kick them away from the login page
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  // If they are a guest, let them see the login/register forms
  return <Outlet />;
}