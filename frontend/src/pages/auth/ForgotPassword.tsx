import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '@/services/authApi';
import { useFormSubmit } from '@/hooks/useFormSubmit';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '@/schemas/auth';
import { AuthCard } from '@/components/common/AuthCard';
import { InputField } from '@/components/common';
import { LoadingButton } from '@/components/common/LoadingButton';

export default function ForgotPassword() {
  const navigate = useNavigate();

  const { isSubmitting, handleSubmit } = useFormSubmit({
    errorMessage: 'Something went wrong. Please try again.',
  });

  const { register, handleSubmit: handleSubmitForm, formState: { errors } } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = (data: ForgotPasswordFormData) =>
    handleSubmit(async () => {
      const message = await authApi.forgotPassword(data);
      return message;
    }, {
      successMessageFn: (result) => result,
      onSuccess: () => {
        navigate('/reset-password', { state: { email: data.email } });
      },
    });

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
      <form onSubmit={handleSubmitForm(onSubmit)} className="space-y-4" noValidate>
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
          isLoading={isSubmitting}
          loadingText="Sending code..."
        >
          Send Reset Code
        </LoadingButton>
      </form>
    </AuthCard>
  );
}
