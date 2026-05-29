import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { toast } from 'sonner';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common/FormField';
import { LoadingButton } from '@/components/common/LoadingButton';

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = async (data: ForgotPasswordFormData) => {
    setIsLoading(true);
    try {
      const message = await authApi.forgotPassword(data);
      toast.success(message);
      navigate('/reset-password', { state: { email: data.email } });
    } catch (error: any) {
      const errorData = error.response?.data;
      const errorMessage = errorData?.errors?.[0]
        || errorData?.message
        || 'Something went wrong. Please try again.';
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthCard
      icon="🔑"
      title="Forgot Password?"
      subtitle="Enter your email and we'll send you a reset code."
      footer={
        <Link to="/login" className="text-gray-500 hover:underline">
          Back to Sign In
        </Link>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <InputField
          label="Email"
          htmlFor="email"
          type="email"
          placeholder="john@canteen.com"
          error={errors.email}
          register={register('email')}
        />

        <LoadingButton
          type="submit"
          className="w-full"
          isLoading={isLoading}
          loadingText="Sending code..."
        >
          Send Reset Code
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
