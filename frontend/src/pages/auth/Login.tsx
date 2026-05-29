import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useAuthStore } from '@/store/authStore';
import { toast } from 'sonner';
import { loginSchema, type LoginFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common/FormField';
import { LoadingButton } from '@/components/common/LoadingButton';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function Login() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      const { accessToken, ...user } = await authApi.login(data);
      setAuth(user, accessToken);
      toast.success('Welcome back!');
      navigate('/dashboard');
    } catch (error: any) {
      const errorData = error.response?.data;
      const errorMessage = errorData?.errors?.[0]
        || errorData?.message
        || 'Failed to login. Please check your credentials.';

      if (errorMessage.toLowerCase().includes('verify your email')) {
        toast.error('You need to verify your email first.');
        navigate('/verify-email', { state: { email: data.email } });
      } else {
        toast.error(errorMessage);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthCard
      title="Canteen System"
      subtitle="Sign in to your account"
      footer={
        <span className="text-gray-500">
          Don't have an account?{' '}
          <Link to="/register" className="text-blue-600 hover:underline">Register here</Link>
        </span>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <InputField
          label="Email"
          htmlFor="email"
          type="email"
          placeholder="namra@canteen.com"
          error={errors.email}
          register={register('email')}
        />

        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="password">Password</Label>
            <Link to="/forgot-password" className="text-sm text-blue-600 hover:underline">
              Forgot password?
            </Link>
          </div>
          <Input
            id="password"
            type="password"
            {...register('password')}
          />
          {errors.password && <p className="text-red-500 text-xs">{errors.password.message}</p>}
        </div>

        <LoadingButton
          type="submit"
          className="w-full"
          isLoading={isLoading}
          loadingText="Signing in..."
        >
          Sign In
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
