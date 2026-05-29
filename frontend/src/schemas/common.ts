import { z } from 'zod';

export const emailSchema = z
  .string()
  .min(1, 'Email is required')
  .email('Invalid email address');

export const companyEmailSchema = emailSchema.regex(
  /^[a-zA-Z0-9._%+-]+@tatvasoft\.com$/,
  'Only company emails are allowed'
);

export const passwordSchema = z
  .string()
  .min(8, 'Password must be at least 8 characters');

export const phoneSchema = z
  .string()
  .max(16, 'Phone number cannot exceed 16 characters')
  .refine((val) => val === '' || /^\+?[0-9]{7,16}$/.test(val), {
    message: 'Invalid phone number. Must be 7-16 digits with an optional + prefix',
  })
  .transform((val) => (val === '' ? null : val))
  .nullable();

export const otpSchema = z
  .string()
  .length(6, 'OTP must be exactly 6 digits')
  .regex(/^\d{6}$/, 'OTP must contain only digits');
