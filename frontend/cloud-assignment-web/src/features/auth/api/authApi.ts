import { apiRequest } from '../../../services/api/apiClient';
import type {
  AuthSession,
  ChangePasswordPayload,
  LoginPayload,
} from '../types/authTypes';

export const authApi = {
  login(payload: LoginPayload) {
    return apiRequest<AuthSession>('/auth/login', {
      method: 'POST',
      body: payload,
    });
  },

  refresh() {
    return apiRequest<AuthSession>('/auth/refresh', {
      method: 'POST',
    });
  },

  logout() {
    return apiRequest<void>('/auth/logout', {
      method: 'POST',
    });
  },

  getCurrentUser(accessToken: string) {
    return apiRequest<AuthSession['user']>('/auth/me', { accessToken });
  },

  changePassword(accessToken: string, payload: ChangePasswordPayload) {
    return apiRequest<void>('/auth/change-password', {
      method: 'POST',
      accessToken,
      body: payload,
    });
  },
};
