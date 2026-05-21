import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

export default function ProtectedRoute() {
  const { isAuthenticated, isInitialized } = useAuthStore();

  // Wait for the silent refresh to finish before deciding where to send them
  if (!isInitialized) {
    return (
      <div className="flex h-screen w-screen items-center justify-center">
        <div className="animate-pulse text-lg font-medium text-gray-500">
          Verifying session...
        </div>
      </div>
    );
  }

  // If not logged in, kick them to login and replace the history stack
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // If logged in, render the child components (like the Dashboard)
  return <Outlet />;
}