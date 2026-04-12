import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react';
import { authApi, tokenStore, usersApi } from '../api/client';
import type { LoginRequest, RegisterRequest, UserDto } from '../types/api';

interface AuthState {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthActions {
  login:    (data: LoginRequest)    => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout:   ()                      => void;
  clearError: ()                    => void;
}

// Combine state + actions into a single context type
type AuthContextType = AuthState & AuthActions;

const AuthContext = createContext<AuthContextType | null>(null);

// Key used to persist the token across page reloads.
// Using sessionStorage (cleared when the tab closes) balances UX vs XSS risk.
const SESSION_KEY = 'bl_jwt';

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,   // true on mount while we re-hydrate token
    error: null,
  });

  // ── Re-hydrate token from sessionStorage on first load ────────────────
  useEffect(() => {
    const stored = sessionStorage.getItem(SESSION_KEY);
    if (!stored) {
      setState(s => ({ ...s, isLoading: false }));
      return;
    }

    tokenStore.set(stored);
    usersApi
      .me()
      .then(user => {
        setState({ user, isAuthenticated: true, isLoading: false, error: null });
      })
      .catch(() => {
        // Token is stale/invalid — discard it
        sessionStorage.removeItem(SESSION_KEY);
        tokenStore.clear();
        setState({ user: null, isAuthenticated: false, isLoading: false, error: null });
      });
  }, []);

  // ── Login ─────────────────────────────────────────────────────────────
  const login = useCallback(async (data: LoginRequest) => {
    setState(s => ({ ...s, isLoading: true, error: null }));
    try {
      const { token, ...user } = await authApi.login(data);
      tokenStore.set(token);
      sessionStorage.setItem(SESSION_KEY, token);
      setState({ user: user as UserDto, isAuthenticated: true, isLoading: false, error: null });
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Login failed';
      setState(s => ({ ...s, isLoading: false, error: msg }));
      throw e;
    }
  }, []);

  // ── Register ──────────────────────────────────────────────────────────
  const register = useCallback(async (data: RegisterRequest) => {
    setState(s => ({ ...s, isLoading: true, error: null }));
    try {
      await authApi.register(data);
      setState(s => ({ ...s, isLoading: false }));
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Registration failed';
      setState(s => ({ ...s, isLoading: false, error: msg }));
      throw e;
    }
  }, []);

  // ── Logout ────────────────────────────────────────────────────────────
  const logout = useCallback(() => {
    sessionStorage.removeItem(SESSION_KEY);
    tokenStore.clear();
    setState({ user: null, isAuthenticated: false, isLoading: false, error: null });
  }, []);

  const clearError = useCallback(() => {
    setState(s => ({ ...s, error: null }));
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, login, register, logout, clearError }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within <AuthProvider>');
  return ctx;
}
