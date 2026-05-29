import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { toast } from 'sonner';
import type { RegisterResponse } from '@/types/api';
import { registerSchema, type RegisterFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common/FormField';
import { LoadingButton } from '@/components/common/LoadingButton';

export default function Register() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    try {
      const result = await authApi.register(data) as RegisterResponse;
      toast.success(result.message);
      navigate('/verify-email', { state: { email: result.email } });
    } catch (error: any) {
      const errorData = error.response?.data;
      const errorMessage = errorData?.errors?.[0]
        || errorData?.message
        || 'Failed to register. Please try again.';
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthCard
      title="Create an Account"
      subtitle="Join the Canteen System"
      footer={
        <span className="text-gray-500">
          Already have an account?{' '}
          <Link to="/login" className="text-blue-600 hover:underline font-medium">Sign in here</Link>
        </span>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <InputField
          label="Full Name"
          htmlFor="name"
          placeholder="John Doe"
          error={errors.name}
          register={register('name')}
        />

        <InputField
          label="Email"
          htmlFor="email"
          type="email"
          placeholder="john@canteen.com"
          error={errors.email}
          register={register('email')}
        />

        <InputField
          label="Password"
          htmlFor="password"
          type="password"
          error={errors.password}
          register={register('password')}
        />

        <LoadingButton
          type="submit"
          className="w-full mt-2"
          isLoading={isLoading}
          loadingText="Creating account..."
        >
          Create Account
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
