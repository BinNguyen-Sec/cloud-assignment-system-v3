export type UserRole = 'Admin' | 'Teacher' | 'Student';

export interface AuthenticatedUser {
  id: string;
  studentCode: string | null;
  fullName: string;
  email: string;
  role: UserRole;
  mustChangePassword: boolean;
}

export interface AuthSession {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthenticatedUser;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
