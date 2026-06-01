import { lazy, Suspense } from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import GuestRoute from './GuestRoute';
import MainLayout from '@/components/layout/MainLayout';

const Login = lazy(() => import('@/pages/auth/Login'));
const Register = lazy(() => import('@/pages/auth/Register'));
const ForgotPassword = lazy(() => import('@/pages/auth/ForgotPassword'));
const ResetPassword = lazy(() => import('@/pages/auth/ResetPassword'));
const VerifyEmail = lazy(() => import('@/pages/auth/VerifyEmail'));
const Dashboard = lazy(() => import('@/pages/dashboard/Dashboard'));
const Profile = lazy(() => import('@/pages/profile/Profile'));
const Cart = lazy(() => import('@/pages/Cart'));

function PageLoader() {
  return (
    <div className="flex h-screen items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-orange-500 border-t-transparent" />
    </div>
  );
}

function SuspenseWrapper({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<PageLoader />}>{children}</Suspense>;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/dashboard" replace />,
  },
  {
    element: <GuestRoute />,
    children: [
      {
        path: '/login',
        element: <SuspenseWrapper><Login /></SuspenseWrapper>,
      },
      {
        path: '/register',
        element: <SuspenseWrapper><Register /></SuspenseWrapper>,
      },
      {
        path: '/verify-email',
        element: <SuspenseWrapper><VerifyEmail /></SuspenseWrapper>,
      },
      {
        path: '/forgot-password',
        element: <SuspenseWrapper><ForgotPassword /></SuspenseWrapper>,
      },
      {
        path: '/reset-password',
        element: <SuspenseWrapper><ResetPassword /></SuspenseWrapper>,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <MainLayout />,
        children: [
          {
            path: '/dashboard',
            element: <SuspenseWrapper><Dashboard /></SuspenseWrapper>,
          },
          {
            path: '/profile',
            element: <SuspenseWrapper><Profile /></SuspenseWrapper>,
          },
          {
            path: '/cart',
            element: <SuspenseWrapper><Cart /></SuspenseWrapper>,
          }
        ],
      },
    ],
  },
]);
