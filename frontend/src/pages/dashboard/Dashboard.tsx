import { useAuthStore } from '@/store/authStore';
import { authApi } from '@/services/authApi';
import { Button } from '@/components/ui/button';
import { useNavigate } from 'react-router-dom';

export default function Dashboard() {
  const { user, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate('/login');
    authApi.logout().catch(() => {});
  };

  return (
    <div className="flex h-screen flex-col items-center justify-center bg-gray-50">
      <div className="p-8 bg-white rounded-xl shadow-sm text-center border border-gray-100">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Secure Dashboard</h1>
        <p className="text-gray-500 mb-6">Welcome back, {user?.name}</p>

        <div className="bg-gray-900 text-green-400 p-4 rounded-lg mb-6 text-sm text-left">
          <pre>{JSON.stringify(user, null, 2)}</pre>
        </div>

        <Button onClick={handleLogout} variant="destructive">
          Logout
        </Button>
      </div>
    </div>
  );
}
