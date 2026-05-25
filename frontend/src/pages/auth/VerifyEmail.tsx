import { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useAuthStore } from '@/store/authStore';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function VerifyEmail() {
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((state) => state.setAuth);

  // Email passed from Register page via navigate state
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
    } catch (error: any) {
      toast.error('Failed to resend OTP. Please try again.');
    } finally {
      setIsResending(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 py-12 px-4">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="mb-8 text-center">
          <div className="text-4xl mb-3">📧</div>
          <h1 className="text-2xl font-bold text-gray-900">Verify Your Email</h1>
          <p className="text-gray-500 text-sm mt-1">
            We've sent a 6-digit code to your email.
          </p>
        </div>

        <form onSubmit={handleVerify} className="space-y-4" noValidate>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="john@example.com"
              disabled={!!emailFromState}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="otpCode">Verification Code</Label>
            <Input
              id="otpCode"
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={otpCode}
              onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, ''))}
              placeholder="000000"
              className="text-center text-2xl tracking-[0.5em] font-mono"
            />
          </div>

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? 'Verifying...' : 'Verify Email'}
          </Button>
        </form>

        <div className="mt-6 text-center text-sm">
          <span className="text-gray-500">Didn't receive the code? </span>
          <button
            onClick={handleResendOtp}
            disabled={isResending}
            className="text-blue-600 hover:underline font-medium disabled:text-gray-400"
          >
            {isResending ? 'Sending...' : 'Resend OTP'}
          </button>
        </div>

        <div className="mt-3 text-center text-sm">
          <Link to="/login" className="text-gray-500 hover:underline">
            Back to Sign In
          </Link>
        </div>
      </div>
    </div>
  );
}