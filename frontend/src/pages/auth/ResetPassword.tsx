import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useFormSubmit } from '@/hooks/useFormSubmit';
import { resetPasswordSchema, type ResetPasswordFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common';
import { OTPInput } from '@/components/common/OTPInput';
import { LoadingButton } from '@/components/common/LoadingButton';

export default function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();

  const emailFromState = (location.state as { email?: string })?.email || '';

  const { isSubmitting, handleSubmit } = useFormSubmit({
    errorMessage: 'Failed to reset password. Please check your OTP and try again.',
  });

  const { register, handleSubmit: handleSubmitForm, formState: { errors } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: emailFromState,
    }
  });

  const onSubmit = (data: ResetPasswordFormData) =>
    handleSubmit(async () => {
      const message = await authApi.resetPassword(data);
      return message;
    }, {
      successMessageFn: (result) => result,
      onSuccess: () => {
        navigate('/login');
      },
    });

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
      <form onSubmit={handleSubmitForm(onSubmit)} className="space-y-4" noValidate>
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
          isLoading={isSubmitting}
          loadingText="Resetting..."
        >
          Reset Password
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
