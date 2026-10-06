import { z } from 'zod';

const slugRegex = /^[a-z0-9-]+$/;

export const createTenantSchema = z.object({
  name: z.string().min(1, 'Name is required.').max(200),
  slug: z.string().min(1, 'Slug is required.').max(64).regex(slugRegex, 'Slug may only contain lowercase letters, digits, and hyphens.'),
  contactEmail: z.string().email('Enter a valid email.').max(200).optional().or(z.literal('')),
  contactPhone: z.string().max(30).optional().or(z.literal('')),
  timeZone: z.string().min(1, 'Time zone is required.').max(100),
  locale: z.string().min(2).max(10),
  currency: z.string().min(3).max(10),
  subscriptionPlanId: z.string().uuid().optional().or(z.literal('')),
  adminFirstName: z.string().min(1, 'First name is required.').max(100),
  adminLastName: z.string().min(1, 'Last name is required.').max(100),
  adminEmail: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
});
export type CreateTenantFormValues = z.infer<typeof createTenantSchema>;

export const updateTenantSchema = z.object({
  name: z.string().min(1).max(200),
  contactEmail: z.string().email().optional().or(z.literal('')),
  contactPhone: z.string().max(30).optional().or(z.literal('')),
  logoUrl: z.string().max(500).url().optional().or(z.literal('')),
  timeZone: z.string().min(1).max(100),
  locale: z.string().min(2).max(10),
  currency: z.string().min(3).max(10),
});
export type UpdateTenantFormValues = z.infer<typeof updateTenantSchema>;

export const suspendTenantSchema = z.object({
  reason: z.string().min(1, 'Provide a reason.').max(500),
});
export type SuspendTenantFormValues = z.infer<typeof suspendTenantSchema>;

export const assignPlanSchema = z.object({
  subscriptionPlanId: z.string().uuid('Choose a plan.'),
  expiresAt: z.string().optional().or(z.literal('')),
});
export type AssignPlanFormValues = z.infer<typeof assignPlanSchema>;
