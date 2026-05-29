import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { toast } from 'sonner';
import { resetPasswordSchema, type ResetPasswordFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common/FormField';
import { OTPInput } from '@/components/common/OTPInput';
import { LoadingButton } from '@/components/common/LoadingButton';

export default function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();

  const emailFromState = (location.state as { email?: string })?.email || '';

  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: emailFromState,
    }
  });

  const onSubmit = async (data: ResetPasswordFormData) => {
    setIsLoading(true);
    try {
      const message = await authApi.resetPassword(data);
      toast.success(message);
      navigate('/login');
    } catch (error: any) {
      const errorData = error.response?.data;
      const errorMessage = errorData?.errors?.[0]
        || errorData?.message
        || 'Failed to reset password. Please check your OTP and try again.';
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthCard
      icon="🔒"
      title="Reset Password"
      subtitle="Enter the 6-digit code sent to your email and your new password."
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
          placeholder="john@example.com"
          readOnly={!!emailFromState}
          error={errors.email}
          register={register('email')}
        />

        <OTPInput
          label="Verification Code"
          htmlFor="otpCode"
          error={errors.otpCode}
          register={register('otpCode')}
        />

        <InputField
          label="New Password"
          htmlFor="newPassword"
          type="password"
          error={errors.newPassword}
          register={register('newPassword')}
        />

        <InputField
          label="Confirm Password"
          htmlFor="confirmPassword"
          type="password"
          error={errors.confirmPassword}
          register={register('confirmPassword')}
        />

        <LoadingButton
          type="submit"
          className="w-full mt-2"
          isLoading={isLoading}
          loadingText="Resetting..."
        >
          Reset Password
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
