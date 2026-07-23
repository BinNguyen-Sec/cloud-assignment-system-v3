import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import { ApiError } from '../../../services/api/apiClient';
import { authApi } from '../api/authApi';
import type {
  AuthenticatedUser,
  ChangePasswordPayload,
  LoginPayload,
} from '../types/authTypes';

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

interface AuthContextValue {
  status: AuthStatus;
  user: AuthenticatedUser | null;
  accessToken: string | null;
  login: (payload: LoginPayload) => Promise<AuthenticatedUser>;
  logout: () => Promise<void>;
  changePassword: (payload: ChangePasswordPayload) => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);
let initialSessionPromise: ReturnType<typeof authApi.refresh> | null = null;

function loadInitialSession() {
  initialSessionPromise ??= authApi.refresh();
  return initialSessionPromise;
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    loadInitialSession()
      .then((session) => {
        if (!active) return;
        setUser(session.user);
        setAccessToken(session.accessToken);
        setStatus('authenticated');
      })
      .catch((error: unknown) => {
        if (!active) return;
        if (!(error instanceof ApiError) || error.status !== 401) {
          console.error('Authentication bootstrap failed.', error);
        }
        setUser(null);
        setAccessToken(null);
        setStatus('anonymous');
      });

    return () => {
      active = false;
    };
  }, []);

  const login = useCallback(async (payload: LoginPayload) => {
    const session = await authApi.login(payload);
    setUser(session.user);
    setAccessToken(session.accessToken);
    setStatus('authenticated');
    return session.user;
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      setAccessToken(null);
      setStatus('anonymous');
      initialSessionPromise = null;
    }
  }, []);

  const changePassword = useCallback(
    async (payload: ChangePasswordPayload) => {
      if (!accessToken) {
        throw new Error('Authentication token is unavailable.');
      }

      await authApi.changePassword(accessToken, payload);
      setUser(null);
      setAccessToken(null);
      setStatus('anonymous');
      initialSessionPromise = null;
    },
    [accessToken],
  );

  const value = useMemo<AuthContextValue>(
    () => ({ status, user, accessToken, login, logout, changePassword }),
    [accessToken, changePassword, login, logout, status, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext<AuthContextValue | null>(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }

  return context;
}
