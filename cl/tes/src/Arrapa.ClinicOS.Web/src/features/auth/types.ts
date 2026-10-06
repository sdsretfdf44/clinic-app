export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl: string | null;
  phoneNumber: string | null;
  tenantId: string;
  tenantSlug: string;
  tenantName: string;
  primaryRole: string;
  roles: string[];
  lastLoginAt: string | null;
}

export interface AuthTokens {
  accessToken: string;
  tokenType: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: AuthUser;
}
