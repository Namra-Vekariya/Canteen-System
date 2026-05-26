import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { resetPasswordSchema, type ResetPasswordFormData } from '@/schemas/auth';

export default function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();

  // Email passed from ForgotPassword page via navigate state
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
      navigate('/login'); // Redirect to login on success
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
    <div className="flex min-h-screen items-center justify-center bg-gray-50 py-12 px-4">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="mb-8 text-center">
          <div className="text-4xl mb-3">🔒</div>
          <h1 className="text-2xl font-bold text-gray-900">Reset Password</h1>
          <p className="text-gray-500 text-sm mt-1">
            Enter the 6-digit code sent to your email and your new password.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="john@example.com"
              {...register('email')}
              readOnly={!!emailFromState} // Make readonly if passed from previous page
              className={emailFromState ? "bg-gray-100 text-gray-500 cursor-not-allowed" : ""}
            />
            {errors.email && <p className="text-red-500 text-xs">{errors.email.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="otpCode">Verification Code</Label>
            <Input
              id="otpCode"
              type="text"
              inputMode="numeric"
              maxLength={6}
              placeholder="000000"
              className="text-center text-lg tracking-widest font-mono"
              {...register('otpCode')}
            />
            {errors.otpCode && <p className="text-red-500 text-xs">{errors.otpCode.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="newPassword">New Password</Label>
            <Input id="newPassword" type="password" {...register('newPassword')} />
            {errors.newPassword && <p className="text-red-500 text-xs">{errors.newPassword.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirm Password</Label>
            <Input id="confirmPassword" type="password" {...register('confirmPassword')} />
            {errors.confirmPassword && <p className="text-red-500 text-xs">{errors.confirmPassword.message}</p>}
          </div>

          <Button type="submit" className="w-full mt-2" disabled={isLoading}>
            {isLoading ? 'Resetting...' : 'Reset Password'}
          </Button>
        </form>

        <div className="mt-6 text-center text-sm">
          <Link to="/login" className="text-gray-500 hover:underline">
            Back to Sign In
          </Link>
        </div>
      </div>
    </div>
  );
}