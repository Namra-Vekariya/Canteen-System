import {z}  from "zod";

export const loginSchema = z.object({
    email: z.string().min(1, 'Email is required').email('Invalid email address'),
    password: z.string().min(8, 'Password must be at least 8 characters'),
  });
  
  export const registerSchema = z.object({
    name: z.string().min(1, 'Full name is required').max(100, 'Name is too long'),
    email: z.string().min(1, 'Email is required').email('Invalid email address').regex(
      /^[a-zA-Z0-9._%+-]+@tatvasoft\.com$/, 
      "Only company emails are allowed").max(255, 'Email is too long'),
    password: z.string().min(8, 'Password must be at least 8 characters'),
  });
  
  export const verifyEmailSchema = z.object({
    email: z.string().email(),
    otpCode: z.string().length(6, 'OTP must be exactly 6 digits').regex(/^\d{6}$/, 'OTP must contain only digits'),
  });
  
  export const forgotPasswordSchema = z.object({
    email: z.string().min(1, 'Email is required').email('Invalid email address'),
  });
  
  export const resetPasswordSchema = z.object({
    email: z.string().email(),
    otpCode: z.string().length(6, 'OTP must be exactly 6 digits').regex(/^\d{6}$/, 'OTP must contain only digits'),
    newPassword: z.string().min(8, 'Password must be at least 8 characters'),
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
