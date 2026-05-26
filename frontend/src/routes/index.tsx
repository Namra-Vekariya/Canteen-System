import { createBrowserRouter, Navigate } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import Login from '@/pages/auth/Login';
import Dashboard from '@/pages/dashboard/Dashboard';
import Register from '@/pages/auth/Register';
import GuestRoute from './GuestRoute';
import ForgotPassword from '@/pages/auth/ForgotPassword';
import ResetPassword from '@/pages/auth/ResetPassword';
import VerifyEmail from '@/pages/auth/VerifyEmail';
import MainLayout from '@/components/layout/MainLayout';
import Profile from '@/pages/profile/Profile';

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
        element: <Login />,
      },
      {
        path: '/register',
        element: <Register />,
      },
      
      {
        path: '/verify-email',
        element: <VerifyEmail />,
      },
      {
        path: '/forgot-password',
        element: <ForgotPassword />,
      },
      {
        path: '/reset-password',
        element: <ResetPassword />,
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
            element: <Dashboard />,
          },
          {
            path: '/profile',
            element: <Profile />, // We will build this next!
          },
        ],
      },
    ],
  },
]);