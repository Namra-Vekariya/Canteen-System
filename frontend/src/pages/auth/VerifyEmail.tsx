import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { toast } from 'sonner';
import { authApi } from '@/services/authApi';
import { useAuth } from '@/hooks/useAuth';
import { useFormSubmit } from '@/hooks/useFormSubmit';
import { verifyEmailSchema, type VerifyEmailFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { OTPInput } from '@/components/common/OTPInput';
import { LoadingButton } from '@/components/common/LoadingButton';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function VerifyEmail() {
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuth } = useAuth();

  const state = location.state as { email?: string } | null;
  const emailFromState = state?.email ?? '';

  const { register, handleSubmit, formState: { errors } } = useForm<VerifyEmailFormData>({
    resolver: zodResolver(verifyEmailSchema),
    defaultValues: {
      email: emailFromState,
      otpCode: '',
    },
  });

  const { isSubmitting: isVerifying, handleSubmit: handleVerifySubmit } = useFormSubmit({
    errorMessage: 'Verification failed. Please try again.',
  });

  const { isSubmitting: isResending, handleSubmit: handleResendSubmit } = useFormSubmit({
    errorMessage: 'Failed to resend OTP. Please try again.',
  });

  const onVerify = async (data: VerifyEmailFormData) =>
    handleVerifySubmit(async () => {
      const { accessToken, ...user } = await authApi.verifyEmail(data);
      setAuth(user, accessToken);
      return user;
    }, {
      successMessage: 'Email verified. Welcome!',
      onSuccess: () => navigate('/dashboard'),
    });

  const emailValue = emailFromState || '';

  const onResend = async () => {
    if (!emailValue) {
      toast.error('Please enter your email first.');
      return;
    }

    handleResendSubmit(async () => {
      const message = await authApi.resendVerificationOtp(emailValue);
      return message;
    }, {
      successMessageFn: (result) => result,
    });
  };

  return (
    <AuthCard
      icon="📧"
      title="Verify Your Email"
      subtitle="We've sent a 6-digit code to your email."
      footer={
        <div className="space-y-3">
          <p className="text-gray-500">
            Didn't receive the code?{' '}
            <button
              onClick={onResend}
              disabled={isResending}
              className="text-blue-600 hover:underline font-medium disabled:text-gray-400"
            >
              {isResending ? 'Sending...' : 'Resend OTP'}
            </button>
          </p>
          <p>
            <Link to="/login" className="text-gray-500 hover:underline">
              Back to Sign In
            </Link>
          </p>
        </div>
      }
    >
      <form onSubmit={handleSubmit(onVerify)} className="space-y-4" noValidate>
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            placeholder="john@example.com"
            disabled={!!emailFromState}
            {...register('email')}
          />
          {errors.email && <p className="text-red-500 text-xs">{errors.email.message}</p>}
        </div>

        <OTPInput
          label="Verification Code"
          htmlFor="otpCode"
          error={errors.otpCode}
          register={register('otpCode')}
        />

        <LoadingButton
          type="submit"
          className="w-full"
          isLoading={isVerifying}
          loadingText="Verifying..."
        >
          Verify Email
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
