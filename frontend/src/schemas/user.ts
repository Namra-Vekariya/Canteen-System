import { z } from 'zod';

export const updateProfileSchema = z.object({
  name: z.string()
    .min(1, 'Full name is required')
    .max(100, 'Name cannot exceed 100 characters'),
  phone: z.string()
    .max(16, 'Phone number cannot exceed 16 characters')
    .refine((val) => val === '' || /^\+?[0-9]{7,16}$/.test(val), {
      message: 'Invalid phone number. Must be 7-16 digits with an optional + prefix',
    })
    .transform((val) => (val === '' ? null : val)) // Converts empty input to null for C#
    .nullable(),
});

export type UpdateProfileFormData = z.infer<typeof updateProfileSchema>;