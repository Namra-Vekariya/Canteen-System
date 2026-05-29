import { z } from 'zod';
import { phoneSchema } from './common';

export const updateProfileSchema = z.object({
  name: z.string()
    .min(1, 'Full name is required')
    .max(100, 'Name cannot exceed 100 characters'),
  phone: phoneSchema,
});

export type UpdateProfileFormData = z.infer<typeof updateProfileSchema>;
