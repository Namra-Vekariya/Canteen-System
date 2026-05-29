import { z } from 'zod';
import { emailSchema, companyEmailSchema, passwordSchema, otpSchema } from './common';

export const loginSchema = z.object({
  email: emailSchema,
  password: passwordSchema,
});

export const registerSchema = z.object({
  name: z.string().min(1, 'Full name is required').max(100, 'Name is too long'),
  email: companyEmailSchema.max(255, 'Email is too long'),
  password: passwordSchema,
});

export const verifyEmailSchema = z.object({
  email: emailSchema,
  otpCode: otpSchema,
});

export const forgotPasswordSchema = z.object({
  email: emailSchema,
});

export const resetPasswordSchema = z.object({
  email: emailSchema,
  otpCode: otpSchema,
  newPassword: passwordSchema,
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

export type LoginFormData = z.input<typeof loginSchema>;
export type RegisterFormData = z.input<typeof registerSchema>;
export type VerifyEmailFormData = z.input<typeof verifyEmailSchema>;
export type ForgotPasswordFormData = z.input<typeof forgotPasswordSchema>;
export type ResetPasswordFormData = z.input<typeof resetPasswordSchema>;
