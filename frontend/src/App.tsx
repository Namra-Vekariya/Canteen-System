import { useEffect} from 'react'
import { RouterProvider } from 'react-router-dom'
import { Toaster, } from './components/ui/sonner'
import { router } from './routes'
import { useAuthStore } from './store/authStore'
import { useCartStore } from './store/cartStore'
import { authApi } from './services/authApi'
import './App.css'

function App() {
  const { setAuth, clearAuth, setInitialized, isInitialized } = useAuthStore();

  useEffect(() => {
    const performSilentRefresh = async () => {
      try {
        const { accessToken, ...user } = await authApi.refresh();
        setAuth(user, accessToken);
      } catch (error) {
        // If it fails, they just aren't logged in. Clear state.
        clearAuth();
        useCartStore.getState().clearCart();
      } finally {
        // Tell the app it's done checking so it can render the UI
        setInitialized();
      }
    };

    performSilentRefresh();
  }, [setAuth, clearAuth, setInitialized]);

  if (!isInitialized) {
    return <div className="flex h-screen items-center justify-center">Loading session...</div>;
  }

  return (
    <>
      <RouterProvider router={router}/>
      <Toaster position="top-right" richColors />
    </>
  )
}

export default App
