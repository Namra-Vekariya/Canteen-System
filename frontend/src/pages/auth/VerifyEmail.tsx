import { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useAuthStore } from '@/store/authStore';
import { toast } from 'sonner';
import { AuthCard } from '@/components/common/AuthCard';
import { OTPInput } from '@/components/common/OTPInput';
import { LoadingButton } from '@/components/common/LoadingButton';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function VerifyEmail() {
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((state) => state.setAuth);

  const emailFromState = (location.state as { email?: string })?.email || '';

  const [email, setEmail] = useState(emailFromState);
  const [otpCode, setOtpCode] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isResending, setIsResending] = useState(false);

  const handleVerify = async (e: React.SyntheticEvent) => {
    e.preventDefault();

    if (!email || otpCode.length !== 6) {
      toast.error('Please enter your email and 6-digit OTP.');
      return;
    }

    setIsLoading(true);
    try {
      const { accessToken, ...user } = await authApi.verifyEmail({ email, otpCode });
      setAuth(user, accessToken);
      toast.success('Email verified. Welcome!');
      navigate('/dashboard');
    } catch (error: any) {
      const errorData = error.response?.data;
      const errorMessage = errorData?.errors?.[0]
        || errorData?.message
        || 'Verification failed. Please try again.';
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResendOtp = async () => {
    if (!email) {
      toast.error('Please enter your email first.');
      return;
    }

    setIsResending(true);
    try {
      const message = await authApi.resendVerificationOtp(email);
      toast.success(message);
    } catch {
      toast.error('Failed to resend OTP. Please try again.');
    } finally {
      setIsResending(false);
    }
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
              onClick={handleResendOtp}
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
      <form onSubmit={handleVerify} className="space-y-4" noValidate>
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            placeholder="john@example.com"
            disabled={!!emailFromState}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>

        <OTPInput
          label="Verification Code"
          htmlFor="otpCode"
          value={otpCode}
          onChange={setOtpCode}
        />

        <LoadingButton
          type="submit"
          className="w-full"
          isLoading={isLoading}
          loadingText="Verifying..."
        >
          Verify Email
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
