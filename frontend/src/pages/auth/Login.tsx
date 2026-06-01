import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useAuth } from '@/hooks/useAuth';
import { useFormSubmit } from '@/hooks/useFormSubmit';
import { getApiErrorMessage } from '@/lib/utils';
import { loginSchema, type LoginFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common/FormField';
import { LoadingButton } from '@/components/common/LoadingButton';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function Login() {
  const navigate = useNavigate();
  const { setAuth } = useAuth();

  const { isSubmitting, handleSubmit } = useFormSubmit({
    errorMessage: 'Failed to login. Please check your credentials.',
  });

  const { register, handleSubmit: handleSubmitForm, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (data: LoginFormData) =>
    handleSubmit(async () => {
      const { accessToken, ...user } = await authApi.login(data);
      setAuth(user, accessToken);
      return user;
    }, {
      successMessage: 'Welcome back!',
      onSuccess: () => {
        navigate('/dashboard');
      },
      onError: (error) => {
        const msg = getApiErrorMessage(error, '');
        if (msg.toLowerCase().includes('verify your email')) {
          navigate('/verify-email', { state: { email: data.email } });
        }
      },
    });

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
      <form onSubmit={handleSubmitForm(onSubmit)} className="space-y-4" noValidate>
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
          isLoading={isSubmitting}
          loadingText="Signing in..."
        >
          Sign In
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
