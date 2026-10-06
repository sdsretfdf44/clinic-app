export interface TenantSummary {
  id: string;
  name: string;
  slug: string;
  status: string;
  contactEmail: string | null;
  subscriptionPlanName: string | null;
  userCount: number;
  createdOn: string;
  suspendedAt: string | null;
}

export interface TenantDetail {
  id: string;
  name: string;
  slug: string;
  status: string;
  contactEmail: string | null;
  contactPhone: string | null;
  logoUrl: string | null;
  timeZone: string;
  locale: string;
  currency: string;
  subscriptionPlanId: string | null;
  subscriptionPlanName: string | null;
  subscriptionExpiresAt: string | null;
  suspendedAt: string | null;
  suspensionReason: string | null;
  userCount: number;
  createdOn: string;
  updatedOn: string | null;
}
